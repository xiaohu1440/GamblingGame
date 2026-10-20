#requires -Version 5.1
<#
.SYNOPSIS
    GamblingGame 的 Unity 无界面验证脚本：编译检查 / 跑测试 / 构建。

.DESCRIPTION
    给 AI Agent 或本地 CI 用的统一入口。默认工程根 = 本脚本上两级（Tools\Agent\..\..）。
    退出码：0 通过 / 1 失败 / 2 环境问题（找不到 Unity、工程被占用、没有测试用例）。

    同一个工程不能同时被 Unity Editor 和本脚本打开。Editor 开着时请改用 MCP 桥。

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-ci.ps1 -Task check

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-ci.ps1 -Task test -TestPlatform EditMode
#>
[CmdletBinding()]
param(
    # 不传则用脚本上两级的工程根
    [string]$ProjectPath,

    # 不传则按 ProjectVersion.txt 自动探测
    [string]$UnityExe,

    [ValidateSet('check', 'test', 'build')]
    [string]$Task = 'check',

    [ValidateSet('EditMode', 'PlayMode')]
    [string]$TestPlatform = 'EditMode',

    # PlayMode 测试通常需要图形，默认不加 -nographics
    [switch]$NoGraphics
)

$ErrorActionPreference = 'Stop'

function Fail-Env([string]$Message) {
    Write-Host "[unity-ci] ENV  $Message" -ForegroundColor Red
    exit 2
}

# ---------------------------------------------------------------- 定位工程
if (-not $ProjectPath) {
    $ProjectPath = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
}
if (-not (Test-Path -LiteralPath $ProjectPath)) { Fail-Env "工程不存在: $ProjectPath" }
$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path

$versionFile = Join-Path $ProjectPath 'ProjectSettings\ProjectVersion.txt'
if (-not (Test-Path -LiteralPath $versionFile)) {
    Fail-Env "不是 Unity 工程（缺 ProjectSettings\ProjectVersion.txt）: $ProjectPath"
}

$editorVersion = $null
foreach ($line in Get-Content -LiteralPath $versionFile) {
    if ($line -match '^m_EditorVersion:\s*(\S+)') { $editorVersion = $Matches[1]; break }
}
if (-not $editorVersion) { Fail-Env "无法从 ProjectVersion.txt 读出编辑器版本" }

# ---------------------------------------------------------------- 定位 Unity
function Resolve-UnityExe([string]$Explicit, [string]$Version) {
    if ($Explicit) {
        if (-not (Test-Path -LiteralPath $Explicit)) { Fail-Env "指定的 Unity.exe 不存在: $Explicit" }
        return (Resolve-Path -LiteralPath $Explicit).Path
    }
    $candidates = @(
        "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
        "C:\Program Files\Unity\$Version\Editor\Unity.exe"
        "D:\Unity\unity\$Version\Editor\Unity.exe"
        "D:\Unity\Hub\Editor\$Version\Editor\Unity.exe"
        "E:\Unity\unity\$Version\Editor\Unity.exe"
    )
    foreach ($c in $candidates) {
        if (Test-Path -LiteralPath $c) { return (Resolve-Path -LiteralPath $c).Path }
    }
    Fail-Env "找不到 Unity $Version。请用 -UnityExe 指定，或确认版本已安装。"
}

$unity = Resolve-UnityExe -Explicit $UnityExe -Version $editorVersion

# ---------------------------------------------------------------- 日志目录
$logDir = Join-Path $ProjectPath 'Logs\agent'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'

function Invoke-Unity([string[]]$UnityArgs, [string]$LogPath) {
    Write-Host "[unity-ci] $unity" -ForegroundColor DarkGray
    Write-Host "[unity-ci] $($UnityArgs -join ' ')" -ForegroundColor DarkGray
    $all = @('-batchmode', '-projectPath', "`"$ProjectPath`"") + $UnityArgs + @('-logFile', "`"$LogPath`"")
    $proc = Start-Process -FilePath $unity -ArgumentList $all -Wait -PassThru -NoNewWindow
    return $proc.ExitCode
}

function Show-LogErrors([string]$LogPath) {
    if (-not (Test-Path -LiteralPath $LogPath)) { return }
    $errors = Select-String -LiteralPath $LogPath -Pattern 'error CS\d+|Scripts have compiler errors|Unhandled exception|Aborting batchmode' -ErrorAction SilentlyContinue
    if ($errors) {
        Write-Host "[unity-ci] ---- 关键错误 ----" -ForegroundColor Yellow
        $errors | Select-Object -First 40 | ForEach-Object { Write-Host ("  " + $_.Line.Trim()) }
        Write-Host "[unity-ci] 完整日志: $LogPath"
    }
}

# ---------------------------------------------------------------- check
if ($Task -eq 'check') {
    $log = Join-Path $logDir "check-$stamp.log"
    $runArgs = @('-quit')
    if ($NoGraphics) { $runArgs += '-nographics' }
    # 无 -executeMethod 时 Unity 只做导入 + 编译；有编译错误会以非 0 退出
    $code = Invoke-Unity -UnityArgs $runArgs -LogPath $log
    Show-LogErrors -LogPath $log
    if ($code -ne 0) {
        Write-Host "[unity-ci] FAIL  编译/导入失败 (exit $code)，日志: $log" -ForegroundColor Red
        exit 1
    }
    Write-Host "[unity-ci] OK    编译通过 (Unity $editorVersion)" -ForegroundColor Green
    exit 0
}

# ---------------------------------------------------------------- test
if ($Task -eq 'test') {
    $log = Join-Path $logDir "test-$TestPlatform-$stamp.log"
    $xml = Join-Path $logDir "test-$TestPlatform-$stamp.xml"
    # -runTests 自己会退出，不要加 -quit
    $runArgs = @('-runTests', '-testPlatform', $TestPlatform.ToLower(), '-testResults', "`"$xml`"")
    if ($NoGraphics) { $runArgs += '-nographics' }
    $code = Invoke-Unity -UnityArgs $runArgs -LogPath $log
    Show-LogErrors -LogPath $log

    if (-not (Test-Path -LiteralPath $xml)) {
        Write-Host "[unity-ci] FAIL  未生成测试结果（Unity 崩溃或工程被占用）exit=$code 日志: $log" -ForegroundColor Red
        exit 1
    }

    [xml]$doc = Get-Content -LiteralPath $xml -Raw
    $run = $doc.SelectSingleNode('/test-run')
    $total = $run.GetAttribute('total')
    $passed = $run.GetAttribute('passed')
    $failed = $run.GetAttribute('failed')
    $skipped = $run.GetAttribute('skipped')
    $duration = $run.GetAttribute('duration')

    Write-Host "[unity-ci] $TestPlatform  total=$total passed=$passed failed=$failed skipped=$skipped duration=${duration}s"

    if ([int]$total -eq 0) {
        Write-Host "[unity-ci] ENV  没有测试用例。本工程的游戏代码在 Assembly-CSharp，asmdef 无法引用默认程序集；" -ForegroundColor Yellow
        Write-Host "              要测试需先把纯逻辑抽到独立 asmdef，再建 Assets\Tests\EditMode。" -ForegroundColor Yellow
        Write-Host "[unity-ci] 结果文件: $xml"
        exit 2
    }

    if ([int]$failed -gt 0) {
        Write-Host "[unity-ci] ---- 失败用例 ----" -ForegroundColor Yellow
        foreach ($case in $doc.SelectNodes("//test-case[@result='Failed']")) {
            $name = $case.GetAttribute('fullname')
            $msgNode = $case.SelectSingleNode('.//message')
            $msg = if ($msgNode) { ($msgNode.InnerText -split "`n")[0] } else { '' }
            Write-Host ("  FAIL  $name")
            if ($msg) { Write-Host ("        " + $msg.Trim()) -ForegroundColor DarkGray }
        }
        Write-Host "[unity-ci] 结果文件: $xml" -ForegroundColor Red
        exit 1
    }

    Write-Host "[unity-ci] OK    测试全部通过" -ForegroundColor Green
    exit 0
}

# ---------------------------------------------------------------- build
if ($Task -eq 'build') {
    $agentCi = Join-Path $ProjectPath 'Assets\Editor\AgentCI.cs'
    if (-not (Test-Path -LiteralPath $agentCi)) {
        Fail-Env "缺少 $agentCi。把它从 unity-agent-kit\unity\Editor\AgentCI.cs 拷到 Assets\Editor\ 后再跑构建。"
    }
    $log = Join-Path $logDir "build-$stamp.log"
    $runArgs = @('-quit', '-executeMethod', 'AgentTools.Editor.AgentCI.BuildWindows')
    if ($NoGraphics) { $runArgs += '-nographics' }
    $code = Invoke-Unity -UnityArgs $runArgs -LogPath $log
    Show-LogErrors -LogPath $log

    $exe = Join-Path $ProjectPath 'Builds\StandaloneWindows64\Game.exe'
    if ($code -ne 0 -or -not (Test-Path -LiteralPath $exe)) {
        Write-Host "[unity-ci] FAIL  构建失败 (exit $code)，日志: $log" -ForegroundColor Red
        exit 1
    }
    $size = [math]::Round((Get-Item -LiteralPath $exe).Length / 1MB, 1)
    Write-Host "[unity-ci] OK    构建产物 ${size}MB -> $exe" -ForegroundColor Green
    exit 0
}

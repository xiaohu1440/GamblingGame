#requires -Version 5.1
<#
.SYNOPSIS
    把本次代码改动写成一条可追溯的变更记录，落到 DescriptionOfChanges\<月>\。

.DESCRIPTION
    DSH 每次生成/修改代码后都应该跑一次，产出「时间 + 具体脚本 + 具体代码片段 + 修改描述」。

    默认记录「工作区相对 HEAD 的改动」（已暂存 + 未暂存 + 未跟踪），
    也可以指定 -Files 精确记录某几个文件，或 -Commit 记录某次提交。

    同一天的多次改动合并进同一个文件，用 --- 分隔：
    输出路径：DescriptionOfChanges\<MM>\<yyyy-MM-dd>.md

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\change-log.ps1 `
        -Summary "新增遗物：紫色筹码翻倍" `
        -Detail "在 RelicSystem 里补了触发点，新增 PurpleChipsEffect 资产类。"

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\change-log.ps1 -Summary "修转动逻辑" -Files Assets/Scripts/Game/GamblingGround.cs
#>
[CmdletBinding()]
param(
    # 一句话说明这次改了什么（会进记录标题）
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Summary,

    # 补充说明：为什么改、影响范围、验证方式
    [string]$Detail = '',

    # 只记录这些文件（相对仓库根的路径）；不给就取工作区全部改动
    [string[]]$Files,

    # 记录某次提交的改动（与 -Files 二选一）
    [string]$Commit,

    # 连文档/配置一起记录（默认只记代码类文件）
    [switch]$AllFiles,

    # 新增文件最多摘录多少行
    [int]$MaxSnippetLines = 60
)

$ErrorActionPreference = 'Stop'

# 通过 powershell -File 传数组时，逗号列表会当成一个字符串，这里统一拆开
if ($Files) {
    $Files = @($Files | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}

$repo = (& git -C $PSScriptRoot rev-parse --show-toplevel 2>$null)
if (-not $repo) {
    Write-Host "[change-log] 找不到 git 仓库根（当前脚本目录：$PSScriptRoot）" -ForegroundColor Red
    exit 1
}
$repo = $repo.Trim()

function Invoke-Git {
    param([string[]]$GitArgs)
    & git -C $repo @GitArgs
}

$branch = (Invoke-Git @('rev-parse', '--abbrev-ref', 'HEAD')).Trim()

# ---------------------------------------------------------------- 收集改动
$codeExtensions = @('.cs', '.asmdef', '.shader', '.hlsl', '.cginc', '.uss', '.uxml', '.ps1', '.json')
$ignorePrefixes = @('DescriptionOfChanges/', 'Library/', 'Logs/', 'Temp/', 'Builds/', 'obj/')

$entries = New-Object System.Collections.ArrayList

if ($Commit) {
    $scope = "提交 $Commit"
    foreach ($line in (Invoke-Git @('show', '--name-status', '--pretty=format:', $Commit))) {
        if (-not $line -or -not $line.Trim()) { continue }
        $parts = $line -split "`t"
        if ($parts.Count -lt 2) { continue }
        [void]$entries.Add([pscustomobject]@{ Status = $parts[0].Trim(); Path = $parts[-1].Trim() })
    }
}
elseif ($Files) {
    $scope = '指定文件'
    foreach ($f in $Files) {
        # 指定路径也要走一次 git status：新增但未 add 的文件是 ??，不是 M
        $status = 'M'
        foreach ($line in (Invoke-Git @('status', '--porcelain', '-uall', '--', $f))) {
            if ($line -and $line.Length -ge 4) { $status = $line.Substring(0, 2).Trim(); break }
        }
        [void]$entries.Add([pscustomobject]@{ Status = $status; Path = ($f -replace '\\', '/') })
    }
}
else {
    $scope = '工作区未提交变更'
    foreach ($line in (Invoke-Git @('status', '--porcelain', '-uall'))) {
        if (-not $line -or $line.Length -lt 4) { continue }
        $status = $line.Substring(0, 2).Trim()
        $path = $line.Substring(3).Trim()
        if ($path.StartsWith('"') -and $path.EndsWith('"')) { $path = $path.Trim('"') }
        [void]$entries.Add([pscustomobject]@{ Status = $status; Path = ($path -replace '\\', '/') })
    }
}

# 过滤：忽略产物/记录目录，默认只留代码类文件
$filtered = @($entries | Where-Object {
    $path = $_.Path
    $ignored = $false
    foreach ($prefix in $ignorePrefixes) {
        if ($path.StartsWith($prefix)) { $ignored = $true; break }
    }
    if ($ignored) { return $false }
    if ($AllFiles) { return $true }
    $ext = [System.IO.Path]::GetExtension($path).ToLowerInvariant()
    return ($codeExtensions -contains $ext)
})

if ($filtered.Count -eq 0) {
    Write-Host "[change-log] 没有检测到需要记录的代码改动（范围：$scope）" -ForegroundColor Yellow
    exit 1
}

# ---------------------------------------------------------------- 片段渲染
function Get-Language {
    param([string]$Path)
    switch ([System.IO.Path]::GetExtension($Path).ToLowerInvariant()) {
        '.cs' { return 'csharp' }
        '.asmdef' { return 'json' }
        '.json' { return 'json' }
        '.ps1' { return 'powershell' }
        '.shader' { return 'hlsl' }
        '.hlsl' { return 'hlsl' }
        '.cginc' { return 'hlsl' }
        '.uss' { return 'css' }
        '.uxml' { return 'xml' }
        default { return 'text' }
    }
}

function Get-FileSection {
    param([string]$Path, [string]$Status, [int]$Index)
    $lang = Get-Language -Path $Path
    $full = Join-Path $repo $Path
    $text = ''

    if ($Status -eq '??' -or $Status -eq 'A') {
        if (Test-Path -LiteralPath $full) {
            $lines = Get-Content -LiteralPath $full -Encoding UTF8
            $take = [Math]::Min($lines.Count, $MaxSnippetLines)
            $body = ($lines[0..($take - 1)] -join "`n")
            $tail = if ($lines.Count -gt $take) { "`n// …… 其余 $($lines.Count - $take) 行省略" } else { '' }
            $text = "新增文件，摘录前 $take 行：`n`n``````$lang`n$body$tail`n``````"
        }
        else {
            $text = '新增文件（构建产物已清理，无内容可摘录）。'
        }
        $label = '新增'
    }
    elseif ($Status -eq 'D') {
        $text = "文件已删除。`n`n``````diff`n" + ((Invoke-Git @('show', "HEAD:$Path")) -join "`n") + "`n``````"
        $label = '删除'
    }
    else {
        $diff = if ($Commit) { (Invoke-Git @('show', $Commit, '--unified=3', '--', $Path)) } else { (Invoke-Git @('diff', 'HEAD', '--unified=3', '--', $Path)) }
        $diffText = ($diff -join "`n")
        if (-not $diffText.Trim()) {
            $diffText = ((Invoke-Git @('diff', '--cached', '--unified=3', '--', $Path)) -join "`n")
        }
        if (-not $diffText.Trim()) {
            $text = '（diff 为空：可能是仅权限/行尾变化，或文件已被后续提交处理）'
        }
        else {
            $text = "``````diff`n$diffText`n``````"
        }
        $label = '修改'
    }

    return @"
#### $Index. ``$Path``（$label）

$text
"@
}

# ---------------------------------------------------------------- 组装（按天合并）
$now = Get-Date
$month = $now.ToString('MM')
$dir = Join-Path (Join-Path $repo 'DescriptionOfChanges') $month
if (-not (Test-Path -LiteralPath $dir)) {
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
}

# 同一天的所有改动写进同一个文件：2026-10-19.md
$dayFile = Join-Path $dir ($now.ToString('yyyy-MM-dd') + '.md')
$utf8 = New-Object System.Text.UTF8Encoding($true)

$existing = ''
$index = 1
if (Test-Path -LiteralPath $dayFile) {
    $existing = [System.IO.File]::ReadAllText($dayFile, [System.Text.Encoding]::UTF8)
    # 已有几个「## 变更 N」，下一个就是 N+1
    $index = ([regex]::Matches($existing, '(?m)^##\s+变更\s+\d+')).Count + 1
}

$sections = New-Object System.Collections.ArrayList
for ($i = 0; $i -lt $filtered.Count; $i++) {
    [void]$sections.Add((Get-FileSection -Path $filtered[$i].Path -Status $filtered[$i].Status -Index ($i + 1)))
}

$fileList = ($filtered | ForEach-Object { "- ``$($_.Path)```（$($_.Status)）" }) -join "`n"
$detailBlock = if ($Detail) { "### 说明`n`n$Detail`n" } else { '' }

$sectionText = @"
## 变更 $index · $($now.ToString('HH:mm:ss')) ($($now.ToString('zzz')))

| 项 | 值 |
|---|---|
| 摘要 | $Summary |
| 范围 | $scope（$($filtered.Count) 个文件） |
| 分支 | $branch |

### 涉及脚本

$fileList

$detailBlock
### 代码片段

$($sections -join "`n")
"@

if (-not $existing) {
    $header = @"
# 代码变更记录 · $($now.ToString('yyyy-MM-dd'))

> 同一天的多次改动合并在本文件，用 ``---`` 分隔。
> 由 ``Tools/Agent/change-log.ps1`` 自动生成，勿手改。

$sectionText
"@
    [System.IO.File]::WriteAllText($dayFile, $header, $utf8)
}
else {
    $trimmed = $existing.TrimEnd()
    $appended = $trimmed + "`n`n---`n`n" + $sectionText
    [System.IO.File]::WriteAllText($dayFile, $appended, $utf8)
}

Write-Host "[change-log] 已写入：$dayFile" -ForegroundColor Green
Write-Host "[change-log] 第 $index 次变更，记录 $($filtered.Count) 个文件（范围：$scope）"
exit 0

#requires -Version 5.1
<#
.SYNOPSIS
    QFramework 合规检查：扫描核心玩法脚本里常见的「绕开框架」写法。

.DESCRIPTION
    只做高信号、低误报的几类检查，用来拦住新代码踩坑：

      QF001  FindObjectOfType / FindObjectsOfType   → 依赖应该走序列化引用或 Architecture
      QF002  Resources.Load(...)                    → 资源应走 ResKit / ResLoader
      QF003  new XxxSystem(...)                     → 系统应通过 Architecture 注册、GetSystem 获取
      QF004  public/internal static 可变字段         → 全局状态应放 Global 的 BindableProperty

    存量代码里有历史违规，默认只报告、不失败（退出码 0）。
    加 -Strict 则任何命中都返回 1，可挂到提交前的检查里。

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\qframework-lint.ps1

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\qframework-lint.ps1 -Strict
#>
[CmdletBinding()]
param(
    # 命中即失败（默认只报告）
    [switch]$Strict,

    # 扫描根目录，默认仓库根下的 Assets
    [string]$Root,

    # 每类最多列出多少条明细
    [int]$MaxPerRule = 10
)

$ErrorActionPreference = 'Stop'

$repo = (& git -C $PSScriptRoot rev-parse --show-toplevel 2>$null)
if ($repo) { $repo = $repo.Trim() } else { $repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot) }

if (-not $Root) { $Root = Join-Path $repo 'Assets' }

$scanDirs = @(
    (Join-Path $Root 'Scripts'),
    (Join-Path $Root 'Art\UI\Scripts')
) | Where-Object { Test-Path -LiteralPath $_ }

$files = @()
foreach ($dir in $scanDirs) {
    $files += Get-ChildItem -LiteralPath $dir -Recurse -File -Filter *.cs |
        Where-Object { $_.FullName -notmatch '\\QFramework\\' }
}

$rules = @(
    @{ Id = 'QF001'; Pattern = '\bFindObjects?OfType\s*<'; Message = 'FindObjectOfType：依赖应走序列化引用 / Architecture，不要运行时全局查找' },
    @{ Id = 'QF002'; Pattern = '\bResources\.Load'; Message = 'Resources.Load：资源应走 ResKit / ResLoader（或 UIKit 面板）' },
    @{ Id = 'QF003'; Pattern = '\bnew\s+(RelicSystem|EnchantmentSystem)\s*\('; Message = 'new XxxSystem：系统应由 Architecture 注册，用 this.GetSystem<T>() 获取' },
    @{ Id = 'QF004'; Pattern = '^\s*(public|internal)\s+static\s+(?!readonly\b|const\b|class\b|struct\b|void\b|event\b)'; Message = 'static 可变字段：全局状态应放 Global 的 BindableProperty' }
)

$hits = @()
foreach ($file in $files) {
    $rel = $file.FullName.Substring($repo.Length + 1)
    $lineNo = 0
    foreach ($line in Get-Content -LiteralPath $file.FullName -Encoding UTF8) {
        $lineNo++
        foreach ($rule in $rules) {
            if ($line -match $rule.Pattern) {
                if ($rule.Id -eq 'QF004' -and ($line -match 'BindableProperty' -or $line -match '\(')) { continue }
                if ($rule.Id -eq 'QF003' -and $line -match 'RegisterSystem') { continue }
                $hits += [pscustomobject]@{
                    Rule    = $rule.Id
                    Message = $rule.Message
                    File    = $rel
                    Line    = $lineNo
                    Text    = $line.Trim()
                }
                break
            }
        }
    }
}

Write-Host "[qf-lint] 扫描 $($files.Count) 个脚本（$($scanDirs.Count) 个目录）"

foreach ($rule in $rules) {
    $ruleHits = @($hits | Where-Object { $_.Rule -eq $rule.Id })
    if ($ruleHits.Count -eq 0) { continue }

    Write-Host ""
    Write-Host ("  {0}  {1}  -> {2} 处" -f $rule.Id, $rule.Message, $ruleHits.Count) -ForegroundColor Yellow
    $ruleHits | Select-Object -First $MaxPerRule | ForEach-Object {
        Write-Host ("       {0}:{1}   {2}" -f $_.File, $_.Line, $_.Text)
    }
    if ($ruleHits.Count -gt $MaxPerRule) {
        Write-Host ("       …… 其余 {0} 处省略" -f ($ruleHits.Count - $MaxPerRule))
    }
}

Write-Host ""
if ($hits.Count -eq 0) {
    Write-Host "[qf-lint] OK 没有发现绕开 QFramework 的写法" -ForegroundColor Green
    exit 0
}

Write-Host ("[qf-lint] 合计 {0} 处。存量可以保留，但新代码不要再新增；规则见 AGENTS.md §3.1" -f $hits.Count) -ForegroundColor Yellow

if ($Strict) {
    Write-Host "[qf-lint] -Strict 模式：存在命中，返回 1" -ForegroundColor Red
    exit 1
}

exit 0

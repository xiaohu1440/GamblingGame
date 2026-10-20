#requires -Version 5.1
<#
.SYNOPSIS
    需求卡准入检查：按需求级别（L0 / L1 / L2）校验必填项。

.DESCRIPTION
    规则来源：《AI 游戏策划案编写与协作规范》+ Docs/Requirements/README.md。
    级别读卡片「基本信息」里的 `- 级别：L0/L1/L2`；没写按 L2（最严）处理。
    也可以用 -Level 强制指定（与卡片声明不一致时会提示）。

      L0 小细节：功能 ID、改什么、影响面、验收（≥1 条）、待确认
      L1 功能改动：基本信息 + 设计目的 + 范围 + 触发/处理/输出 + 验收（≥3 条）+ 待确认，至少一条 [MUST]
      L2 大功能：完整 10 节 + 8 项必填 + 边界 4 类 + 技术约束

    任何一项不通过 → 退出码 1，执行方不得开始实现，只能先向需求方提问补齐。

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\requirement-check.ps1 -Path Docs\Requirements\SYS-002_商店刷新.md

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\requirement-check.ps1 -Path Docs\Requirements\EX-003_按钮间距.md -Level L0
#>
[CmdletBinding()]
param(
    # 需求卡路径（Markdown）
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Path,

    # 级别：auto = 读卡片里的「级别」，没有则按 L2
    [ValidateSet('auto', 'L0', 'L1', 'L2')]
    [string]$Level = 'auto'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Path)) {
    Write-Host "[req-check] 文件不存在：$Path" -ForegroundColor Red
    exit 1
}

$repo = (& git -C $PSScriptRoot rev-parse --show-toplevel 2>$null)
if ($repo) { $repo = $repo.Trim() } else { $repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot) }

$lines = Get-Content -LiteralPath $Path -Encoding UTF8
$text = $lines -join "`n"

$problems = @()

# ---------------------------------------------------------------- 级别
$declared = $null
if ($text -match '级别\s*[:：]\s*(L[0-2])') { $declared = $Matches[1] }

if ($Level -eq 'auto') {
    $level = if ($declared) { $declared } else { 'L2' }
}
else {
    $level = $Level
    if ($declared -and $declared -ne $Level) {
        Write-Host "[req-check] 注意：卡片声明 $declared，但命令行指定 $Level，按 $Level 校验" -ForegroundColor Yellow
    }
}

$spec = @{
    'L0' = @{
        Sections   = @()
        Fields     = @('功能 ID', '本次包含', '依赖系统', '尚未确认的问题')
        MinChecks  = 1
        NeedMust   = $false
        Boundary   = 0
        Technique  = @()
        Validate   = @{}
    }
    'L1' = @{
        Sections   = @('1. 基本信息', '2. 设计目的', '3. 功能范围', '4. 需求详情', '9. 验收标准', '10. 参考')
        Fields     = @('功能 ID', '文档版本', '状态', '优先级', '解决的问题', '本次包含', '本次不包含',
                       '前置条件', '触发条件', '处理规则', '输出', '尚未确认的问题')
        MinChecks  = 3
        NeedMust   = $true
        Boundary   = 0
        Technique  = @()
        Validate   = @{ '状态' = @('Draft', 'Review', 'Approved', 'Deprecated') }
    }
    'L2' = @{
        Sections   = @('1. 基本信息', '2. 设计目的', '3. 功能范围', '4. 需求详情', '5. 详细流程',
                       '6. 边界与异常', '7. 表现与交互', '8. 数据与技术约束', '9. 验收标准', '10. 参考')
        Fields     = @('功能 ID', '文档版本', '状态', '优先级', '解决的问题', '希望产生的玩家体验',
                       '本次包含', '本次不包含', '前置条件', '触发条件', '输入', '输出', '状态变化',
                       '数值与公式', '尚未确认的问题')
        MinChecks  = 3
        NeedMust   = $true
        Boundary   = 4
        Technique  = @('依赖系统', '必须保持的不变量', '禁止修改的内容')
        Validate   = @{ '状态' = @('Draft', 'Review', 'Approved', 'Deprecated') }
    }
}

$rule = $spec[$level]

# ---------------------------------------------------------------- 章节齐全
foreach ($section in $rule.Sections) {
    if ($text -notmatch [regex]::Escape($section)) {
        $problems += "缺少章节：$section"
    }
}

# ---------------------------------------------------------------- 字段有值
# 支持两种写法：行内「- 依赖系统：xxx」，或冒号后换行、用缩进子列表展开
function Get-FieldValue {
    param([string]$Label)

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $pattern = '^\s*[-*]?\s*' + [regex]::Escape($Label) + '\s*[:：]\s*(.*)$'
        if ($lines[$i] -match $pattern) {
            $inline = $Matches[1].Trim()
            if ($inline.Length -gt 0) { return $inline }

            # 冒号后为空：收集紧跟的缩进子列表（遇到同级或更浅的行就停）
            $block = @()
            for ($j = $i + 1; $j -lt $lines.Count; $j++) {
                $next = $lines[$j]
                if ($next.Trim().Length -eq 0) { continue }
                if ($next -match '^[ \t]+\S') { $block += $next.Trim(); continue }
                break
            }
            if ($block.Count -gt 0) { return ($block -join ' ') }
            return ''
        }
    }

    return $null
}

function Test-Field {
    param([string]$Label, [string[]]$AcceptAny)
    $value = Get-FieldValue -Label $Label
    if ($null -eq $value) {
        $script:problems += "缺少字段：$Label"
        return
    }
    if ($value.Length -eq 0) {
        $script:problems += "字段未填写：$Label"
        return
    }
    if ($value -match '^[<＜\[].*[>＞\]]$') {
        $script:problems += "字段仍是占位符：$Label"
    }
    if ($AcceptAny -and ($AcceptAny -notcontains $value)) {
        $script:problems += "字段取值非法：$Label = $value（应为 $($AcceptAny -join ' / ')）"
    }
}

foreach ($field in $rule.Fields) {
    $accepted = $null
    if ($rule.Validate.ContainsKey($field)) { $accepted = $rule.Validate[$field] }
    Test-Field -Label $field -AcceptAny $accepted
}

# ---------------------------------------------------------------- 规则强度
if ($rule.NeedMust) {
    if ($text -notmatch '\[MUST\]|\[MUST NOT\]|\[MAY\]') {
        $problems += '处理规则里没有任何 [MUST] / [MUST NOT] / [MAY] 标记'
    }
    elseif ($text -notmatch '\[MUST\]|\[MUST NOT\]') {
        $problems += '处理规则里只有 [MAY]，至少需要一条 [MUST] 或 [MUST NOT]'
    }
}

# ---------------------------------------------------------------- 边界
if ($rule.Boundary -gt 0) {
    $edgeLabels = @('无效输入', '资源不足', '重复触发', '中途退出或重进')
    $edgeFilled = 0
    foreach ($label in $edgeLabels) {
        $value = Get-FieldValue -Label $label
        if ($value -and $value.Length -gt 0 -and $value -ne '无') { $edgeFilled++ }
    }
    if ($edgeFilled -lt $rule.Boundary) {
        $problems += "边界与异常覆盖不足：$($rule.Boundary) 类中只填了 $edgeFilled 类"
    }
}

# ---------------------------------------------------------------- 技术约束
foreach ($label in $rule.Technique) {
    $value = Get-FieldValue -Label $label
    if (-not $value -or $value.Length -eq 0) { $problems += "字段未填写：$label" }
}

# ---------------------------------------------------------------- 验收标准
$checks = @($lines | Where-Object { $_ -match '^\s*[-*]?\s*(\[[ xX]\]|☐|□)' })
if ($checks.Count -lt $rule.MinChecks) {
    $problems += "验收标准不足 $($rule.MinChecks) 条可勾选项（当前 $($checks.Count) 条）"
}

# ---------------------------------------------------------------- 输出
$name = Split-Path -Leaf $Path
Write-Host "[req-check] $name（级别 $level）"

if ($problems.Count -eq 0) {
    Write-Host "[req-check] OK 需求卡满足 $level 要求，可以进入实现" -ForegroundColor Green
    exit 0
}

Write-Host "[req-check] 不通过，共 $($problems.Count) 项：" -ForegroundColor Red
foreach ($problem in $problems) {
    Write-Host "  - $problem" -ForegroundColor Yellow
}
Write-Host "[req-check] 补齐后再执行；不要用假设代替需求（见 .dsh/skills/requirement-contract/SKILL.md）" -ForegroundColor Red
exit 1

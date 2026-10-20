#requires -Version 5.1
<#
.SYNOPSIS
    Unity MCP 直连客户端：不经过 DSH，直接和 Editor 里的 MCP 服务（默认 8080）说话。

.DESCRIPTION
    Unity 打开工程后，com.coplaydev.unity-mcp 会在 http://127.0.0.1:8080/mcp 起一个
    streamable-http MCP 服务。这个脚本用同一条链路做健康检查、列工具、调工具，
    用于：

      * DSH 的 mcp-unity 没连上时的手替；
      * 排障（是 Editor 没起服务，还是 DSH 侧配置问题）；
      * CI/批处理里不依赖 AI 客户端的快速检查。

    退出码：0 成功 / 1 调用失败 / 2 服务不可达。

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Action health

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Action tools

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Tool read_console -ToolArgsFile args.json

.NOTES
    从 PowerShell 里调 powershell.exe 时，内联 JSON 的引号会被命令行吃掉
    （'{"a":1}' 变成 {a:1}），所以复杂参数请写进文件用 -ToolArgsFile，
    或在本机控制台里用 --% 停用解析：
    powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Tool read_console --% -ToolArgs {"action":"get","types":["error"]}
#>
[CmdletBinding()]
param(
    [ValidateSet('health', 'tools', 'call')]
    [string]$Action = 'health',

    # -Tool 省略时等价于 -Action call
    [string]$Tool,

    # 工具参数，JSON 字符串。不要叫 $Args：它是 PowerShell 自动变量，会被清空。
    [Alias('Args')]
    [string]$ToolArgs = '{}',

    # 工具参数文件（UTF-8 JSON）；复杂参数优先用这个，避开引号转义
    [string]$ToolArgsFile,

    [string]$Endpoint = 'http://127.0.0.1:8080/mcp',

    [int]$TimeoutSec = 120
)

$ErrorActionPreference = 'Stop'

if ($Tool) { $Action = 'call' }

if ($ToolArgsFile) {
    if (-not (Test-Path -LiteralPath $ToolArgsFile)) {
        Write-Host "[unity-mcp] 找不到参数文件: $ToolArgsFile" -ForegroundColor Red
        exit 1
    }
    $ToolArgs = (Get-Content -LiteralPath $ToolArgsFile -Raw -Encoding UTF8).Trim()
}

$healthUrl = ($Endpoint -replace '/mcp/?$', '') + '/health'

function Get-SseJson {
    param($Response)
    $raw = $Response.Content
    $data = ($raw -split "`n" | Where-Object { $_ -like 'data: *' } | ForEach-Object { $_.Substring(6) }) -join ''
    if (-not $data) { return $null }
    return $data | ConvertFrom-Json
}

# ---------------------------------------------------------------- health
if ($Action -eq 'health') {
    try {
        $h = Invoke-RestMethod -Uri $healthUrl -TimeoutSec 10
    } catch {
        Write-Host "[unity-mcp] DOWN  $healthUrl  ($($_.Exception.Message))" -ForegroundColor Red
        Write-Host "[unity-mcp] 检查：Unity Editor 是否打开、Window > MCP for Unity 里 Server 是否为绿色。" -ForegroundColor Yellow
        exit 2
    }
    Write-Host "[unity-mcp] UP    $healthUrl" -ForegroundColor Green
    if ($h.version) { Write-Host "[unity-mcp] version $($h.version)  status $($h.status)" }
    exit 0
}

# ---------------------------------------------------------------- 建立会话
$initBody = '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"unity-mcp.ps1","version":"1.0"}}}'
try {
    $init = Invoke-WebRequest -Uri $Endpoint -Method POST -Body $initBody -ContentType 'application/json' `
        -Headers @{ Accept = 'application/json, text/event-stream' } -TimeoutSec 15 -UseBasicParsing
} catch {
    Write-Host "[unity-mcp] 无法连接 $Endpoint：$($_.Exception.Message)" -ForegroundColor Red
    exit 2
}

$headers = @{
    Accept         = 'application/json, text/event-stream'
    'mcp-session-id' = $init.Headers['mcp-session-id']
}

Invoke-WebRequest -Uri $Endpoint -Method POST -Body '{"jsonrpc":"2.0","method":"notifications/initialized"}' `
    -ContentType 'application/json' -Headers $headers -TimeoutSec 10 -UseBasicParsing | Out-Null

function Invoke-Rpc {
    param([string]$Json)
    $resp = Invoke-WebRequest -Uri $Endpoint -Method POST -Body $Json -ContentType 'application/json' `
        -Headers $headers -TimeoutSec $TimeoutSec -UseBasicParsing
    $obj = Get-SseJson -Response $resp
    if (-not $obj) { throw '空响应（不是 SSE data 帧）' }
    if ($obj.error) { throw ($obj.error | ConvertTo-Json -Depth 5 -Compress) }
    return $obj
}

# ---------------------------------------------------------------- tools
if ($Action -eq 'tools') {
    $obj = Invoke-Rpc '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'
    $tools = $obj.result.tools
    Write-Host "[unity-mcp] $($tools.Count) tools" -ForegroundColor Green
    $tools | ForEach-Object {
        $first = ($_.description -split "`n")[0]
        if ($first.Length -gt 110) { $first = $first.Substring(0, 110) }
        "  {0,-28} {1}" -f $_.name, $first
    }
    exit 0
}

# ---------------------------------------------------------------- call
# 先自检参数 JSON：powershell.exe 会吃掉内联引号，必须把原因说清楚
try { $null = $ToolArgs | ConvertFrom-Json } catch {
    Write-Host "[unity-mcp] 参数不是合法 JSON：$ToolArgs" -ForegroundColor Red
    Write-Host '            从 PowerShell 调 powershell.exe 时内联引号会被剥离；改用 -ToolArgsFile，或加 --%' -ForegroundColor Yellow
    exit 1
}

$payload = '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"' + $Tool + '","arguments":' + $ToolArgs + '}}'
try {
    $obj = Invoke-Rpc $payload
} catch {
    Write-Host "[unity-mcp] FAIL  $Tool : $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Message -like '*400*') {
        Write-Host '            400 通常是参数 JSON 被 shell 吃掉了引号（{a:1} 而不是 {"a":1}）——改用 -ToolArgsFile。' -ForegroundColor Yellow
    }
    exit 1
}

$text = ($obj.result.content | Where-Object { $_.type -eq 'text' } | ForEach-Object { $_.text }) -join "`n"
if (-not $text -and $obj.result.structuredContent) {
    $text = $obj.result.structuredContent | ConvertTo-Json -Depth 10
}
Write-Output $text

if ($obj.result.isError) { exit 1 }
exit 0

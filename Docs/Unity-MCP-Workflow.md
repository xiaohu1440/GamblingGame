# Unity × DSH × MCP 开发工作流

> 核验时间：2026-10-19 01:10–01:25（Asia/Tokyo）
> 核验对象：`D:\GitProject\GamblingGame`（分支 `日本版本`），Unity 2022.3.62f3
> 本文所有结论都来自对当前工程/当前 Editor 的实测，不是推断。改了 MCP 包或端口请同步更新本文。

---

## 0. 当前工程核验结论

| 项 | 结论 | 证据 |
|---|---|---|
| Unity 版本 | ✅ 2022.3.62f3，Editor 存在 | `ProjectSettings/ProjectVersion.txt`；`D:\Unity\unity\2022.3.62f3\Editor\Unity.exe` |
| 编译状态 | ✅ 0 error / 0 warning | Editor Console 为空；`Library/ScriptAssemblies/Assembly-CSharp.dll` 于 00:38 重建，晚于最新脚本（09-14） |
| 脚本校验 | ✅ `Global.cs` 0 error / 0 warning | MCP `validate_script` |
| 主场景 | ✅ `GamblingMain` 已打开，3 个根节点（Main Camera / GameUIController / GameManager） | MCP `manage_scene get_active` / `get_hierarchy` |
| Build Settings | ✅ 5 个场景启用，顺序与 AGENTS.md 一致 | MCP `manage_scene get_build_settings` |
| 资源完整性 | ✅ 1734 个资源，缺 1 个 `.meta`（`Assets/.qframework/PackageManager/PackageInfosRequestCache.json`，QFramework 缓存，无影响）；无孤儿 `.meta` | 本地扫描 |
| 单元测试 | ✅ `Assets/Tests/EditMode`（`Gambling.Tests.EditMode`）16 个用例，实测 16/16 通过 | MCP `run_tests` |
| 构建 | ✅ `Assets/Editor/AgentCI.cs`，产物 `Builds/StandaloneWindows64/Game.exe`，实测成功（86.2 MB） | `AgentCI.BuildWindows` |
| MCP 包 | ⚠️ `com.coplaydev.unity-mcp` 10.3.0 已装（`Library/PackageCache` 已解析），但 `Packages/manifest.json` + `packages-lock.json` 的改动**未提交** | git status |
| Newtonsoft | ⚠️ 版本冲突：包提供的 13.0.0 生效，QFramework 内置 8.0.0 被忽略（Editor.log 有 Duplicate assembly 警告） | Editor.log |
| MCP 服务 | ✅ 正在运行：`http://127.0.0.1:8080/mcp`，`mcp-for-unity-server` 3.4.9，49 个工具 | `/health` + `tools/list` |
| DSH 接入 | ✅ 已连通：`insert:` + `streamable-http` → 8080，`mcp__unity__*` 49 个工具可用 | profile 已改并重启；`read_console` 实测通过；根因见 §2.1 |

### 为什么 batchmode 现在跑不了

Editor 正开着（`Temp/UnityLockfile` 被持有），`unity-ci.ps1 -Task check` 会报
`another Unity instance is running with this project open`。这是预期行为，不是脚本坏了。
要么关掉 Editor 跑冷通道，要么用 MCP 热通道（本文重点）。

### 另外一个环境坑（已复现）

本会话的 `workspace-write` 沙箱只放行**工程根目录**的写入，`Tools/`、`Logs/`、`Temp/` 等子目录会被拒绝：

```
New-Item : Access to the path 'agent' is denied.
（Tools\Agent\unity-ci.ps1 第 84 行，创建 Logs\agent）
```

Unity 批处理必须写 `Library/`、`Temp/`、`Logs/`，所以**跑 `unity-ci.ps1` 需要完全权限（full access）**，
否则会在启动前就因写日志目录失败而退出。MCP 热通道不受影响（只走 HTTP）。

---

## 1. 两条通道，按场景选

| | MCP 热通道 | batchmode 冷通道 |
|---|---|---|
| 前提 | Unity Editor 开着，MCP 服务在 8080 | Editor 必须关闭 |
| 延迟 | 秒级（复用已编译的域） | 分钟级（每次冷启动 + 全量导入） |
| 能做什么 | 读 Console、改脚本、跑测试、操作场景/预制体/资产、执行菜单、截图、构建 | 只做 编译检查 / 测试 / 构建 三件事 |
| 不能做什么 | Editor 卡死/域重载时调用会超时 | 没法交互式看画面、看 Console |
| 适合 | 日常开发、调玩法、看报错 | 提交前验收、CI |

一句话：**写代码时用热通道，验收时用冷通道（先关 Editor）**。

---

## 2. 把 DSH 接到 Unity MCP（需要改一次配置 + 重启 DSH）

### 2.1 根因：patch 条目写错形状（不是协议、不是服务端）

profile 里原本有一条 `mcp-unity`，但它写在**顶层**：

```yaml
- id: mcp-unity            # ← 错误：这是「覆盖已有条目」的写法
  name: "@deepseek-ai/dsh-mcp-client"
  config: { ... }
```

Cordis 的 patch 数组有三种语义：按 id 覆盖、按 id 禁用、`insert:` 插入。顶层带 `id/name/config`
的条目只会去覆盖**已存在**的同名条目；profile 里没有 `mcp-unity` 这个 id，于是整条被静默忽略——
不报错、不连接、也不打印任何日志。这就是「配置看着没问题，工具就是没有」的真正原因。

官方模板（`@deepseek-ai/dsh-agent-preset/skills/cordis-plugin-development/templates/mcp/cordis.patch.yml`）写得很清楚：

```yaml
- insert:
    - id: demo-mcp
      name: '@deepseek-ai/dsh-mcp-client'
      config: ...
```

（顺便排除掉的其它猜测：8080 的 MCP 服务本身正常，`/health` 是 healthy；用 DSH 同款的
`@modelcontextprotocol/client` 2.2.0 直连能拿到 49 个工具，所以协议版本、SSE、session 头都没问题。）

至于原来的 `stdio + uvx` 方案，即使写对形状也不行：`uvx` 要写 `%LOCALAPPDATA%\uv\cache`，
在沙箱里直接 `Could not acquire lock`。

### 2.2 正确配置：`insert:` + 直连 8080

编辑 `%USERPROFILE%\.dsh\profiles\desktop\cordis.patch.yml`（即 `$env:DSH_PROFILE_DIR\cordis.patch.yml`），
在文件末尾加一段：

```yaml
- insert:
    - id: mcp-unity
      name: "@deepseek-ai/dsh-mcp-client"
      config:
        serverName: unity
        transport: streamable-http
        url: http://127.0.0.1:8080/mcp
        toolCallTimeoutMs: 300000
        failOnStartupError: true
```

依据：`dsh-mcp-client` 的 `Config` schema（`lib/index.js`）对 `streamable-http` 分支要求
`serverName` + `url`，工具以 `mcp__<serverName>__<tool>` 暴露，所以得到的是 `mcp__unity__read_console` 之类。

> **本机已于 2026-10-19 生效**：改完后 DSH 重启，`mcp__unity__*` 全部出现，`read_console` 调用成功。
> 历史备份在 `cordis.patch.yml.bak-20261019-*`（012521 是 http 版但形状错，013725 是更早的 stdio 版）。


改完 **重启 DSH**（配置在启动时读取，热改无效），然后验证：

```powershell
# 1) Unity 侧服务活着？
Invoke-RestMethod http://127.0.0.1:8080/health

# 2) 工具都出来了？（本仓库的小工具，见 §3）
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Action tools
```

**启动顺序**：`failOnStartupError: true` 意味着 DSH 启动那一刻 Unity 的 8080 必须已经在监听。
所以约定「**先开 Unity 工程，再开 DSH**」；如果反了，MCP 条目会明确报错（harness 本身照常启动），
重新开一次 DSH 即可。反过来如果你更想要「连不上也不打扰」，把这一行改成 `false`，代价是又会静默失败。

### 2.3 如果坚持用 stdio

必须让 `uvx` 能写缓存，二选一：

- 预先在**沙箱外**把包装好并把 `UV_CACHE_DIR` 指到可写位置，然后在配置里加 `env: { UV_CACHE_DIR: ... }`；
- 或让 DSH 以完全权限运行（会削弱沙箱，不推荐）。

结论：能用 HTTP 就别用 stdio。HTTP 还省掉「Python 3.12 下载 / 依赖编译 / 包版本漂移」三个不确定性。

### 2.4 Unity 侧的必要条件

- `Packages/manifest.json` 里有 `com.coplaydev.unity-mcp`（当前已有，**记得提交**）；
- Editor 打开后 `Window > MCP for Unity` 里 Server 显示绿色、端口 8080；
  Editor 重启后会自动 auto-start（Editor.log 里能看到 `Server ready on http://127.0.0.1:8080`）；
- 同时开多个 Editor 时，先调 `set_active_instance` 指定目标实例。

---

## 3. 一键自检脚本

`Tools/Agent/unity-mcp.ps1`（不依赖 DSH）：

```powershell
# 健康检查
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Action health

# 列出 49 个工具
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Action tools

# 直接调一个工具（等价于 mcp__unity__read_console）
'{"action":"get","types":["error"],"count":20}' | Set-Content args.json -Encoding ASCII
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Tool read_console -ToolArgsFile args.json
```

> 参数为什么写文件：从 PowerShell 里调 `powershell.exe` 时，内联 JSON 的引号会被命令行吃掉
> （`'{"a":1}'` 传进去变成 `{a:1}`），服务器直接回 400。脚本会提示这一点。
> 复杂参数一律用 `-ToolArgsFile`，简单参数（`{}`）才用 `-ToolArgs`。

DSH 没连上 MCP 时，这个脚本就是「手替」：一样能读 Console、跑测试。

---

## 4. 日常开发回路（热通道）

一个切片（< 200 行）走一遍，每步都有可验证的输出：

1. **改代码** — 用 `edit`/`apply_text_edits`（保留 `.meta`、不碰 YAML）；`create_script` 新建脚本会自动生成 `.meta`。
2. **等编译** — `refresh_unity`（可带等待）或看 Console 出现 `Reloading assemblies`；域重载期间别连发调用，超时就重试一次。
3. **读错误** — `read_console {"types":["error","warning"]}`。编译错误在这里，不在你的编辑器里。
4. **改资产** — `manage_gameobject` / `manage_prefabs` / `manage_asset` / `manage_scriptable_object`，
   一律走 Editor API；**不要**手写 `.unity`/`.prefab`/`.asset` 的 YAML（AGENTS.md 硬规则 3）。
5. **验行为** — `manage_editor play` → `execute_code` 或 `run_tests` → `manage_editor stop`；
   场景/相机用 `manage_scene`、`manage_camera`，需要画面证据时用 `manage_camera` 相关预设截图。
6. **收尾** — `read_console` 确认无新增 warning；`git status` 只含预期文件；再交给冷通道验收。

### 工具速查（49 个里最常用的）

| 目的 | 工具 | 备注 |
|---|---|---|
| 看报错 | `read_console` | `types`/`count`/`filter_text`，按严重级别过滤 |
| 强制重编译 | `refresh_unity` | 可等就绪，避免「改了没生效」 |
| 改现有脚本 | `apply_text_edits` / `script_apply_edits` | 比整文件覆盖安全，保留边界 |
| 新建/删除脚本 | `create_script` / `delete_script` | 走 AssetDatabase，`.meta` 自动处理 |
| 脚本体检 | `validate_script` | 返回 `{warnings, errors}`，可当轻量编译检查 |
| 场景/预制体 | `manage_scene` / `manage_prefabs` | 读层级、改结构、存盘 |
| 资产 | `manage_asset` | 导入/创建/移动/删除 |
| 配置资产 | `manage_scriptable_object` | 按 SerializedObject 路径改字段，改遗物数值就用它 |
| 测试 | `run_tests` → `get_test_job` | 异步，先拿 job_id 再轮询 |
| 构建 | `manage_build` | 不依赖 `AgentCI.cs`，可切平台/构建 |
| 菜单 | `execute_menu_item` | 比如 QFramework 的面板生成命令 |
| 任意 C# | `execute_code` | 万能但危险：只做查证，别用它绕过 AssetDatabase |
| 查 API | `unity_reflect` / `unity_docs` | 不确定 API 是否存在时先问，别猜 |

---

## 5. 验收回路（冷通道，提交前）

```powershell
# 1. 关掉 Unity Editor（必须，否则会报 another Unity instance）
# 2. 关掉后跑：
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-ci.ps1 -Task check      # 0 = 编译通过
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-ci.ps1 -Task test -TestPlatform EditMode
# 16 个 EditMode 用例，退出码 0 = 全绿；2 只表示环境问题（找不到测试/工程被占用）
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-ci.ps1 -Task build
# 产物：Builds/StandaloneWindows64/Game.exe（入口 Assets/Editor/AgentCI.cs）
```

注意：在 DSH 的 `workspace-write` 沙箱里，这个脚本会因为写不了 `Logs\`、`Library\` 而失败
（`Access to the path 'agent' is denied`）。**跑冷通道时请把会话切到完全权限**，或在普通 PowerShell 窗口里跑。

---

## 6. 护栏（和 AGENTS.md 对齐）

- 不改 `Assets/QFramework/**`；不手改 `*.Designer.cs`；不手改场景/预制体/资产 YAML。
- 全局状态只走 `Global`（`Architecture<Global>` + `BindableProperty<T>`）；系统间通信用 `struct` 事件。
- 数值/配置放 ScriptableObject（`Resources/Relics`、`Resources/Data`），不写死。
- 一次一个可验证切片（< 200 行 diff）；提交前确认分支是 `日本版本`。
- 提交前 `git status` 只应有预期文件；`Library/`、`.meta` 大面积变动先停下排查。

---

## 7. 排障对照表

| 现象 | 原因 | 处理 |
|---|---|---|
| 本会话没有 `mcp__unity__*` 工具 | DSH 侧 MCP 未连上（stdio/uvx 或服务未起） | 按 §2.2 改 HTTP 配置并重启 DSH；先 `-Action health` |
| `Could not acquire lock ... uv\cache\.lock` | 沙箱不允许 MCP 子进程写用户缓存 | 改用 HTTP 传输 |
| `another Unity instance is running` | Editor 开着（`Temp/UnityLockfile` 被持有） | 关 Editor，或用 MCP 热通道 |
| MCP 调用超时 | 域重载/正在编译 | 等编译结束，重试一次；仍失败看 `read_console` |
| `/health` 连不上 | Editor 没开或 MCP Server 没启动 | `Window > MCP for Unity` 里看状态，必要时 Start Server |
| `Access to the path 'agent' is denied` | 沙箱只放行工程根目录 | 切完全权限跑 unity-ci |
| `Duplicate assembly 'Newtonsoft.Json.dll'` | 包内 13.0.0 与 QFramework 内置 8.0.0 冲突 | 已知项，包实现生效；改动这一带前先跑一次编译 |
| 改了字段，资产全丢引用 | `RelicEffect` 字段改名没加 `[FormerlySerializedAs]` | 见 AGENTS.md §6 |

---

## 8. 待办

1. `Packages/manifest.json` / `packages-lock.json` 的 unity-mcp 改动还没提交 —— 要么提交，要么回退（AGENTS.md 规则 10：不擅自增删 UPM 包）。
2. ~~没有测试程序集~~ → 已建 `Gambling.Core` + `Gambling.Tests.EditMode`，16 个用例；后续新算法继续往 `Gambling.Core` 里放。
3. ~~缺 `AgentCI.cs`~~ → 已补 `Assets/Editor/AgentCI.cs`，`-Task build` 可用（实测通过）。
4. Newtonsoft 双版本冲突需要一次单独评估（升级/剥离 QFramework 内置 8.0.0 都会牵动大量脚本）。
5. 测试目前只覆盖抽卡权重/挑选这两块纯逻辑；`BetMultiplierSystem`（进度/倍率）也是纯逻辑，但类型被场景序列化，暂不能搬程序集，要覆盖它得先加一层不依赖场景数据的包装。

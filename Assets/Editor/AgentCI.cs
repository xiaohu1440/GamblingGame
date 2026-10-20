using System;
using System.IO;
using System.Linq;
using QFramework;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AgentTools.Editor
{
    /// <summary>
    /// 给 AI / CI 用的构建入口。
    /// 批处理调用：Unity.exe -batchmode -executeMethod AgentTools.Editor.AgentCI.BuildWindows
    /// 产物固定写到 Builds/StandaloneWindows64/Game.exe（.gitignore 已忽略 Builds/）。
    ///
    /// Player 只读真机 AssetBundle；Editor 默认处于模拟模式（SimulateAssetBundles=True），
    /// 改 prefab / 资产不会写进 AB。所以打包前必须先重建 AB，否则包内是旧资源。
    /// </summary>
    public static class AgentCI
    {
        public const string BuildDirectory = "Builds/StandaloneWindows64";
        public const string ExecutablePath = BuildDirectory + "/Game.exe";

        /// <summary>设为 1/true 时跳过 AB 重建，只验证代码（本地临时 / unity-ci.ps1 用）。</summary>
        public const string SkipAssetBundlesEnvVar = "AGENT_SKIP_AB";

        /// <summary>与 ResKit 的 BuildScript 保持一致：输出 AssetBundles/&lt;平台&gt;/，Windows 构建目标为 Windows。</summary>
        private const string AssetBundlePlatformFolder = "Windows";

        /// <summary>Editor 里手动构建用，不退出编辑器；默认先重建 AB。</summary>
        [MenuItem("Tools/Agent/构建 Windows 版")]
        public static void BuildWindowsFromMenu()
        {
            bool ok = RunBuild(false, out string summary);
            EditorUtility.DisplayDialog(ok ? "构建完成" : "构建失败", summary, "好");
        }

        /// <summary>菜单里的「跳过 AB」版：只构建 Player，用于快速验证代码。</summary>
        [MenuItem("Tools/Agent/构建 Windows 版（跳过 AssetBundle 重建）")]
        public static void BuildWindowsSkipAssetBundlesFromMenu()
        {
            bool ok = RunBuild(true, out string summary);
            EditorUtility.DisplayDialog(ok ? "构建完成（未重建 AB）" : "构建失败", summary, "好");
        }

        /// <summary>
        /// 批处理入口。失败时以非 0 退出，unity-ci.ps1 靠退出码和产物是否存在判断成败。
        /// 默认先重建 AB；环境变量 AGENT_SKIP_AB=1/true 可显式跳过（只验代码时用）。
        /// </summary>
        public static void BuildWindows()
        {
            bool ok = RunBuild(IsAssetBundleRebuildSkippedByEnv(), out string summary);

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(ok ? 0 : 1);
                return;
            }

            if (!ok)
            {
                throw new Exception(summary);
            }
        }

        /// <summary>
        /// 供 Editor 内脚本 / 自动化直接调用。<paramref name="skipAssetBundles"/> 为 true 时不重建 AB，
        /// 仅用于「只验证代码」的场景；正常打包请用无参 <see cref="BuildWindows()"/>。
        /// </summary>
        public static void BuildWindows(bool skipAssetBundles)
        {
            bool ok = RunBuild(skipAssetBundles, out string summary);

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(ok ? 0 : 1);
                return;
            }

            if (!ok)
            {
                throw new Exception(summary);
            }
        }

        private static bool IsAssetBundleRebuildSkippedByEnv()
        {
            string value = Environment.GetEnvironmentVariable(SkipAssetBundlesEnvVar);
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static bool RunBuild(bool skipAssetBundles, out string summary)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                summary = "Build Settings 里没有启用的场景，构建取消。";
                Debug.LogError("[AgentCI] " + summary);
                return false;
            }

            try
            {
                Directory.CreateDirectory(BuildDirectory);
            }
            catch (Exception e)
            {
                summary = $"无法创建输出目录 {BuildDirectory}：{e.Message}";
                Debug.LogError("[AgentCI] " + summary);
                return false;
            }

            // 先重建 AB 再打 Player：模拟模式下资源改动只在 AssetDatabase 里，
            // Player 只用真机 AB，不重建就会打出旧资源。
            string assetBundleState;
            if (skipAssetBundles)
            {
                assetBundleState = "跳过";
                Debug.Log($"[AgentCI] 跳过 AssetBundles 重建（skipAssetBundles / {SkipAssetBundlesEnvVar}），直接构建 Player。");
            }
            else
            {
                if (!TryRebuildAssetBundles(out double abSeconds, out string abError))
                {
                    // 不能带着旧 AB 继续打包：直接中止，batchmode 由 BuildWindows 以 1 退出。
                    summary = "AssetBundles 重建失败，已中止打包（避免打出旧资源）。" + abError;
                    Debug.LogError("[AgentCI] " + summary);
                    return false;
                }

                assetBundleState = $"{abSeconds:F1}s";
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = ExecutablePath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            Debug.Log($"[AgentCI] 开始构建 Player，{scenes.Length} 个场景 -> {ExecutablePath}");

            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(options);
            }
            catch (Exception e)
            {
                summary = $"构建抛异常：{e.GetType().Name}: {e.Message}";
                Debug.LogError("[AgentCI] " + summary);
                return false;
            }

            BuildSummary result = report.summary;
            float sizeMb = result.totalSize / 1024f / 1024f;

            if (result.result != BuildResult.Succeeded)
            {
                summary = $"构建失败：{result.result}，错误 {result.totalErrors} 个，耗时 {result.totalTime.TotalSeconds:F0}s。";
                Debug.LogError("[AgentCI] " + summary);
                return false;
            }

            summary = $"构建成功：{ExecutablePath}（{sizeMb:F1} MB，Player {result.totalTime.TotalSeconds:F0}s，AB {assetBundleState}）";
            Debug.Log("[AgentCI] " + summary);
            return true;
        }

        /// <summary>
        /// 用 QFramework ResKit 的编辑器 API 重建 AB。
        /// 注意：该 API 内部吞掉了 BuildPipeline 的返回值，失败只在 Console 报错、不会抛异常，
        /// 所以这里再核对一次产物清单是否真的更新，未更新就按失败处理。
        /// </summary>
        private static bool TryRebuildAssetBundles(out double seconds, out string error)
        {
            seconds = 0;
            error = null;

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string outputManifestPath = Path.Combine(projectRoot, ResKitAssetBundlesOutputPath,
                AssetBundlePlatformFolder, AssetBundlePlatformFolder);
            string streamingManifestPath = Path.Combine(Application.streamingAssetsPath, ResKitAssetBundlesOutputPath,
                AssetBundlePlatformFolder, AssetBundlePlatformFolder);

            DateTime manifestBefore = GetFileWriteTimeUtc(outputManifestPath);

            Debug.Log($"[AgentCI] 开始重建 AssetBundles（平台 {AssetBundlePlatformFolder}，输出 {outputManifestPath}）...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                ResKitEditorAPI.BuildAssetBundles();
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                seconds = stopwatch.Elapsed.TotalSeconds;
                error = $"重建 AssetBundles 失败：{e.GetType().Name}: {e.Message}（耗时 {seconds:F1}s）";
                Debug.LogError("[AgentCI] " + error);
                return false;
            }

            stopwatch.Stop();
            seconds = stopwatch.Elapsed.TotalSeconds;

            bool manifestUpdated = GetFileWriteTimeUtc(outputManifestPath) > manifestBefore;
            bool streamingManifestExists = File.Exists(streamingManifestPath);
            if (!manifestUpdated || !streamingManifestExists)
            {
                error = $"重建 AssetBundles 失败（耗时 {seconds:F1}s）：清单未更新={!manifestUpdated}，"
                    + $"StreamingAssets 清单存在={streamingManifestExists}；详见上方 Unity 报错。";
                Debug.LogError("[AgentCI] " + error);
                return false;
            }

            Debug.Log($"[AgentCI] 重建 AssetBundles 完成，耗时 {seconds:F1}s（{outputManifestPath}）");
            return true;
        }

        private static DateTime GetFileWriteTimeUtc(string path)
        {
            return File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
        }

        /// <summary>ResKit 的 AB 输出根目录（BuildScript 里是项目根下的 AssetBundles）。</summary>
        private const string ResKitAssetBundlesOutputPath = "AssetBundles";
    }
}

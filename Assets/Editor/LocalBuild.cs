// LOCAL ANDROID BUILD ENTRYPOINT (CI-independent). Editor-only build tool — NOT shipped, NOT gameplay.
// Used to produce a device APK locally when GitHub Actions is unavailable (quota/billing), so the Phase-3
// UI can be validated on the physical device. Builds the enabled EditorBuildSettings scene(s) to a debug-signed
// development APK. Does not touch ECS/economy/AI (§12) — it only invokes BuildPipeline.
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class LocalBuild
{
    public static void BuildAndroid()
    {
        // Use the project's bundled Android SDK/NDK/JDK (no external toolchain config needed).
        EditorPrefs.SetBool("SdkUseEmbedded", true);
        EditorPrefs.SetBool("NdkUseEmbedded", true);
        EditorPrefs.SetBool("JdkUseEmbedded", true);

        var enabled = System.Array.FindAll(EditorBuildSettings.scenes, s => s.enabled && !string.IsNullOrEmpty(s.path));
        var paths = System.Array.ConvertAll(enabled, s => s.path);
        if (paths.Length == 0) paths = new[] { "Assets/MainScene.unity" };

        EditorUserBuildSettings.buildAppBundle = false; // APK, not AAB
        System.IO.Directory.CreateDirectory("/tmp/p3build");

        var opts = new BuildPlayerOptions
        {
            scenes = paths,
            locationPathName = "/tmp/p3build/BULWARK.apk",
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.Development, // dev build → auto debug-keystore signing, installable
        };

        Debug.Log("[LOCALBUILD] start; scenes=" + string.Join(",", paths));
        BuildReport report = BuildPipeline.BuildPlayer(opts);
        var s = report.summary;
        Debug.Log($"[LOCALBUILD] result={s.result} errors={s.totalErrors} warnings={s.totalWarnings} sizeBytes={s.totalSize} out={s.outputPath}");
        EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
    }
}
#endif

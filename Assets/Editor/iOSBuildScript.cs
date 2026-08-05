using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class iOSBuildScript
{
    [MenuItem("Build/Build iOS (Xcode Project)")]
    public static void BuildForiOS()
    {
        string buildPath = "build/iOS";
        
        // Command line arguments parsing if provided
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-customBuildPath" && i + 1 < args.Length)
            {
                buildPath = args[i + 1];
            }
        }

        Debug.Log($"[iOSBuildScript] Starting iOS build output to: {buildPath}");

        // Set Bundle Identifier
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.ektgames.perdearkasifutbol");
        Debug.Log("[iOSBuildScript] Set Bundle Identifier to: com.ektgames.perdearkasifutbol");

        // Set AppIcon texture
        Texture2D appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AppIcon1024.png");
        if (appIcon != null)
        {
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new Texture2D[] { appIcon });
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { appIcon });
            Debug.Log("[iOSBuildScript] Successfully set AppIcon texture for iOS target.");
        }

        // Get enabled scenes in build settings
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[iOSBuildScript] No enabled scenes found in Build Settings!");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
            return;
        }

        Debug.Log($"[iOSBuildScript] Building {scenes.Length} scenes:");
        foreach (var scene in scenes)
        {
            Debug.Log($" - {scene}");
        }

        // Ensure output directory exists
        Directory.CreateDirectory(buildPath);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[iOSBuildScript] iOS Xcode project build succeeded! Total size: {summary.totalSize} bytes, Time: {summary.totalTime}");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
        else
        {
            Debug.LogError($"[iOSBuildScript] iOS Xcode project build failed with result: {summary.result}, Errors: {summary.totalErrors}");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}

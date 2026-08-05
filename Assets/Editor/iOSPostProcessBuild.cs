using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class iOSPostProcessBuild
{
    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
            return;

        Debug.Log($"[iOSPostProcessBuild] Processing AppIcon asset catalog for iOS build at: {pathToBuiltProject}");

        string xcassetsPath = Path.Combine(pathToBuiltProject, "Unity-iPhone", "Images.xcassets");
        string appiconsetPath = Path.Combine(xcassetsPath, "AppIcon.appiconset");

        Directory.CreateDirectory(appiconsetPath);

        string sourceIconPath = Path.Combine(Application.dataPath, "AppIcon1024.png");
        string targetIconPath = Path.Combine(appiconsetPath, "AppIcon-1024x1024.png");

        if (File.Exists(sourceIconPath))
        {
            File.Copy(sourceIconPath, targetIconPath, true);
            Debug.Log($"[iOSPostProcessBuild] Copied AppIcon-1024x1024.png to: {targetIconPath}");
        }
        else
        {
            Debug.LogError($"[iOSPostProcessBuild] Source AppIcon not found at: {sourceIconPath}");
        }

        string contentsJsonPath = Path.Combine(appiconsetPath, "Contents.json");
        string contentsJson = @"{
  ""images"": [
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""universal"",
      ""platform"": ""ios"",
      ""size"": ""1024x1024""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ios-marketing"",
      ""scale"": ""1x"",
      ""size"": ""1024x1024""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""iphone"",
      ""scale"": ""2x"",
      ""size"": ""20x20""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""iphone"",
      ""scale"": ""3x"",
      ""size"": ""20x20""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""iphone"",
      ""scale"": ""2x"",
      ""size"": ""29x29""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""iphone"",
      ""scale"": ""3x"",
      ""size"": ""29x29""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""iphone"",
      ""scale"": ""2x"",
      ""size"": ""40x40""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""iphone"",
      ""scale"": ""3x"",
      ""size"": ""40x40""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""iphone"",
      ""scale"": ""2x"",
      ""size"": ""60x60""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""iphone"",
      ""scale"": ""3x"",
      ""size"": ""60x60""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ipad"",
      ""scale"": ""1x"",
      ""size"": ""20x20""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ipad"",
      ""scale"": ""2x"",
      ""size"": ""20x20""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ipad"",
      ""scale"": ""1x"",
      ""size"": ""29x29""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ipad"",
      ""scale"": ""2x"",
      ""size"": ""29x29""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ipad"",
      ""scale"": ""1x"",
      ""size"": ""40x40""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ipad"",
      ""scale"": ""2x"",
      ""size"": ""40x40""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ipad"",
      ""scale"": ""1x"",
      ""size"": ""76x76""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ipad"",
      ""scale"": ""2x"",
      ""size"": ""76x76""
    },
    {
      ""filename"": ""AppIcon-1024x1024.png"",
      ""idiom"": ""ipad"",
      ""scale"": ""2x"",
      ""size"": ""83.5x83.5""
    }
  ],
  ""info"": {
    ""author"": ""xcode"",
    ""version"": 1
  }
}";

        File.WriteAllText(contentsJsonPath, contentsJson);
        Debug.Log($"[iOSPostProcessBuild] Updated AppIcon.appiconset/Contents.json at: {contentsJsonPath}");
    }
}

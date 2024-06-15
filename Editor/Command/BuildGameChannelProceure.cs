using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.LinkerEditor;
using BuildTargetGroup = UnityEditor.BuildTargetGroup;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public class BuildGameChannelProceure : IProcedure
    {
        public object Execute(params object[] args)
        {
            try
            {
                if (args is null || args.Length == 0 || args[0] is not SubGameOptions cfg)
                {
                    return default;
                }

                SetPlayerSetting(cfg);
                BuildPlayerOptions options = new BuildPlayerOptions();
                options.options = BuildOptions.ShowBuiltPlayer;
                options.targetGroup = GetBuildTargetGroup();
                options.target = EditorUserBuildSettings.activeBuildTarget;
                options.scenes = new[] { cfg.scenePath };
                options.locationPathName = GetBuildLocationPath(cfg);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            }
            catch (BuildPlayerWindow.BuildMethodException e)
            {
                Debug.LogError(e);
            }

            EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
            return default;
        }

        private static string GetBuildLocationPath(SubGameOptions channel)
        {
            string platform = AppCore.GetPlatformName();
            string packageName = channel.packageName;
            string version = channel.version;
            string path = $"{AppCore.GetOutputBaseDir()}/packages/{platform}/{version}";
            string fileName = string.Empty;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    fileName = $"{channel.packageName}_{channel.version}_{DateTime.Now.ToString("yyyyMMddHH")}.apk";
                    break;
                default:
                    fileName = Application.productName + ".exe";
                    break;
            }

            if (Directory.Exists(path) is false)
            {
                Directory.CreateDirectory(path);
            }

            return $"{path}/{fileName}";
        }

        private static BuildTargetGroup GetBuildTargetGroup()
        {
            BuildTargetGroup targetGroup = BuildTargetGroup.Unknown;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    targetGroup = BuildTargetGroup.Android;
                    break;
                case BuildTarget.iOS:
                    targetGroup = BuildTargetGroup.iOS;
                    break;
                case BuildTarget.WebGL:
                    targetGroup = BuildTargetGroup.WebGL;
                    break;
                default:
                    targetGroup = BuildTargetGroup.Standalone;
                    break;
            }

            return targetGroup;
        }

        public static void SetPlayerSetting(SubGameOptions channel)
        {
            BuildTargetGroup targetGroup = GetBuildTargetGroup();
            PlayerSettings.SetApplicationIdentifier(targetGroup, channel.packageName);
            PlatformIconKind[] kinds = PlayerSettings.GetSupportedIconKindsForPlatform(targetGroup);
            for (int i = 0; i < kinds.Length; i++)
            {
                PlatformIcon[] platformIcons = PlayerSettings.GetPlatformIcons(targetGroup, kinds[i]);
                for (int j = 0; j < platformIcons.Length; j++)
                {
                    for (int k = 0; k < platformIcons[j].maxLayerCount; k++)
                    {
                        platformIcons[j].SetTexture(channel.icon, k);
                    }
                }

                PlayerSettings.SetPlatformIcons(targetGroup, kinds[i], platformIcons);
            }

            PlayerSettings.companyName = "chaoze";
            PlayerSettings.productName = channel.appName;
            PlayerSettings.bundleVersion = channel.version;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.background = channel.splash;
            PlayerSettings.SplashScreen.show = channel.splash != null;
            PlayerSettings.SplashScreen.overlayOpacity = 1;
            PlayerSettings.SplashScreen.blurBackgroundImage = false;
            PlayerSettings.SplashScreen.backgroundPortrait = null;
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }

        public void Release()
        {
        }
    }
}
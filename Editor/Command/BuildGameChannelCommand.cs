using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using ZGame.Editor.LinkerEditor;
using BuildTargetGroup = UnityEditor.BuildTargetGroup;
using Object = UnityEngine.Object;

namespace ZGame.Editor.Command
{
    public class BuildGameChannelCommand
    {
        public static void Executer(params object[] args)
        {
            var channel = args is not null && args.Length == 1 ? args[0] as PacketOption : default;

            if (channel is null)
            {
                EditorUtility.DisplayDialog("打包失败", "渠道参数错误", "OK");
                return;
            }

            try
            {
                BuildPlayerOptions options = new BuildPlayerOptions();
                options.options = BuildOptions.ShowBuiltPlayer;
                options.targetGroup = GetBuildTargetGroup();
                options.target = EditorUserBuildSettings.activeBuildTarget;
                options.scenes = new[] { "Assets/Startup.unity" };
                options.locationPathName = GetBuildLocationPath(channel, GameConfig.instance);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            }
            catch (BuildPlayerWindow.BuildMethodException e)
            {
                Debug.LogError(e);
            }

            EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
        }

        private static string GetBuildLocationPath(PacketOption channel, GameConfig config)
        {
            string platform = CoreAPI.GetPlatformName();
            string packageName = channel.packageName;
            string version = config.version;
            string path = $"{CoreAPI.GetPlatformOutputBaseDir()}/packages/{platform}/{version}";
            string fileName = string.Empty;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    fileName = $"{channel.packageName}_{config.version}_{DateTime.Now.ToString("yyyyMMddHH")}.apk";
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

        public static void SetPlayerSetting(PacketOption channel, string version)
        {
            Debug.Log("设置渠道包信息：" + channel.title);
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

            PlayerSettings.bundleVersion = version;
            PlayerSettings.companyName = GameConfig.instance.companyName;
            PlayerSettings.productName = channel.appName;
            PlayerSettings.SplashScreen.showUnityLogo = channel.splash != null;
            PlayerSettings.SplashScreen.background = channel.splash;
            PlayerSettings.SplashScreen.overlayOpacity = 1;
            PlayerSettings.SplashScreen.blurBackgroundImage = false;
            PlayerSettings.SplashScreen.backgroundPortrait = null;
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }
    }
}
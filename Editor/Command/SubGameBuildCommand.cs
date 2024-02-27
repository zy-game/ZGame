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
using ZGame.Editor.ResBuild.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.Command
{
    public class SubGameBuildCommand
    {
        public static void Executer(params object[] args)
        {
            if (args.Length != 2)
            {
                return;
            }

            var config = args[0] as EntryConfig;
            var genericAll = args[1] as bool? ?? false;
            //todo 编译资源
            PackageSeting seting = BuilderConfig.instance.packages.Find(x => x.name == config.module);
            AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
            CompileDllCommand.CompileDllActiveBuildTarget();
            LinkerConfig.instance.Generic();
            if (config.mode is CodeMode.Hotfix)
            {
                //todo 编译DLL资源
                if (genericAll)
                {
                    PrebuildCommand.GenerateAll();
                }

                string hotfixPacageDir = GetHotfixPackagePath(seting);
                string aotDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);
                IReadOnlyList<string> aotList = AppDomain.CurrentDomain.GetStaticFieldValue<IReadOnlyList<string>>("AOTGenericReferences", "PatchedAOTAssemblyList");
                if (aotList is null || aotList.Count == 0)
                {
                    EditorUtility.DisplayDialog("提示", "AOT 资源编译失败！", "OK");
                    return;
                }

                byte[] bytes = Zip.Compress("*.dll", aotList.Select(x => $"{aotDir}/{x}").ToArray());
                File.WriteAllBytes($"{hotfixPacageDir}/{config.entryName.ToLower()}_aot.bytes", bytes);

                string hotfixDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget);
                bytes = Zip.Compress("*.dll", hotfixDir);
                File.WriteAllBytes($"{hotfixPacageDir}/{config.entryName.ToLower()}_hotfix.bytes", bytes);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            BuildPackageCommand.Executer(seting);

            try
            {
                BuildPlayerOptions options = new BuildPlayerOptions();
                options.options = BuildOptions.ShowBuiltPlayer;
                options.target = EditorUserBuildSettings.activeBuildTarget;
                options.scenes = new[] { "Assets/Startup.unity" };
                options.locationPathName = $"{BuilderConfig.output}build/{BasicConfig.GetPlatformName()}/{config.currentChannelOptions.packageName}/{config.version}/";
                if (Directory.Exists(options.locationPathName) is false)
                {
                    Directory.CreateDirectory(options.locationPathName);
                }

                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.Android:
                        options.targetGroup = BuildTargetGroup.Android;
                        options.locationPathName += config.currentChannelOptions.packageName + "_" + config.version + ".apk";
                        break;
                    case BuildTarget.iOS:
                        options.targetGroup = BuildTargetGroup.iOS;
                        break;
                    case BuildTarget.StandaloneWindows:
                        options.targetGroup = BuildTargetGroup.Standalone;
                        break;
                }

                PlayerSettings.SetApplicationIdentifier(options.targetGroup, config.currentChannelOptions.packageName);
                PlatformIconKind[] kinds = PlayerSettings.GetSupportedIconKindsForPlatform(options.targetGroup);
                for (int i = 0; i < kinds.Length; i++)
                {
                    PlatformIcon[] platformIcons = PlayerSettings.GetPlatformIcons(options.targetGroup, kinds[i]);
                    for (int j = 0; j < platformIcons.Length; j++)
                    {
                        for (int k = 0; k < platformIcons[j].maxLayerCount; k++)
                        {
                            platformIcons[j].SetTexture(config.currentChannelOptions.icon, k);
                        }
                    }

                    PlayerSettings.SetPlatformIcons(options.targetGroup, kinds[i], platformIcons);
                }

                PlayerSettings.bundleVersion = config.version;
                PlayerSettings.companyName = BasicConfig.instance.companyName;
                PlayerSettings.productName = config.currentChannelOptions.appName;
                PlayerSettings.SplashScreen.showUnityLogo = config.currentChannelOptions.splash != null;
                PlayerSettings.SplashScreen.background = config.currentChannelOptions.splash;
                PlayerSettings.SplashScreen.overlayOpacity = 1;
                PlayerSettings.SplashScreen.blurBackgroundImage = false;
                PlayerSettings.SplashScreen.backgroundPortrait = null;
                PlayerSettings.SplashScreen.showUnityLogo = false;
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            }
            catch (BuildPlayerWindow.BuildMethodException e)
            {
                Debug.LogError(e);
            }

            EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
        }

        private static string GetHotfixPackagePath(PackageSeting seting)
        {
            string hotfixPacageDir = string.Empty;
            RulerData rulerData = seting.items.Find(x => x.folder.name == "Hotfix");
            if (rulerData is not null)
            {
                return AssetDatabase.GetAssetPath(rulerData.folder);
            }

            rulerData = seting.items.Where(x => x.folder != null).FirstOrDefault();
            if (rulerData is null || rulerData.folder == null)
            {
                hotfixPacageDir = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "Hotfix");
            }
            else
            {
                string pacagePath = AssetDatabase.GetAssetPath(rulerData.folder);
                string parent = pacagePath.Substring(0, pacagePath.LastIndexOf("/") + 1);
                hotfixPacageDir = Path.Combine(parent, "Hotfix");
            }

            if (Directory.Exists(hotfixPacageDir) is false)
            {
                Directory.CreateDirectory(hotfixPacageDir);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            seting.items.Add(new RulerData()
            {
                folder = AssetDatabase.LoadAssetAtPath<Object>(hotfixPacageDir),
                buildType = BuildType.Once,
                selector = new Selector(".bytes")
            });
            BuilderConfig.OnSave();
            return hotfixPacageDir;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.LinkerEditor;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.Command
{
    public class BuildHotfixLibraryCommand
    {
        public static void Execute()
        {
            if (GameConfig.instance.mode is not CodeMode.Hotfix)
            {
                return;
            }

            StripAOTDllCommand.GenerateStripedAOTDlls();
            CompileDllCommand.CompileDllActiveBuildTarget();
            LinkerConfig.instance.Generic();

            IReadOnlyList<string> aotList = AppDomain.CurrentDomain.GetStaticFieldValue<IReadOnlyList<string>>("AOTGenericReferences", "PatchedAOTAssemblyList");
            if (aotList is null || aotList.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "AOT 资源编译失败！", "OK");
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(GameConfig.instance.path).ToLower();
            string aotDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);
            byte[] bytes = Zip.Compress("*.dll", aotList.Select(x => $"{aotDir}/{x}").ToArray());
            File.WriteAllBytes(GameFrameworkEntry.GetPlatformOutputPath($"{fileName}_aot.bytes"), bytes);
            bytes = Zip.Compress("*.dll", SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget));
            File.WriteAllBytes(GameFrameworkEntry.GetPlatformOutputPath($"{fileName}_hotfix.bytes"), bytes);
            UploadResourcePackageCommand.Executer(OSSConfig.instance.current, GameFrameworkEntry.GetPlatformOutputPath($"{fileName}_aot.bytes"));
            UploadResourcePackageCommand.Executer(OSSConfig.instance.current, GameFrameworkEntry.GetPlatformOutputPath($"{fileName}_hotfix.bytes"));
            // string hotfixPacageDir = GetHotfixPackagePath(seting);
            // string aotDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);
            //
            //
            // byte[] bytes = Zip.Compress("*.dll", aotList.Select(x => $"{aotDir}/{x}").ToArray());
            // File.WriteAllBytes($"{hotfixPacageDir}/{GameConfig.instance.entryName.ToLower()}_aot.bytes", bytes);
            //
            // string hotfixDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget);
            // bytes = Zip.Compress("*.dll", hotfixDir);
            // File.WriteAllBytes($"{hotfixPacageDir}/{GameConfig.instance.entryName.ToLower()}_hotfix.bytes", bytes);
            // AssetDatabase.SaveAssets();
            // AssetDatabase.Refresh();
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
            BuilderConfig.Save();
            return hotfixPacageDir;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.LinkerEditor;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource;
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

            PackageSeting packageSeting = BuilderConfig.instance.packages.Find(x => x.title == ResConfig.instance.defaultPackageName);
            string aotFileName = $"{Path.GetFileNameWithoutExtension(GameConfig.instance.path).ToLower()}_aot.bytes";
            string hotfixFileName = $"{Path.GetFileNameWithoutExtension(GameConfig.instance.path).ToLower()}_hotfix.bytes";
            string aotDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);
            Zip.ComperssToPath(aotFileName, "*.dll", aotList.Select(x => $"{aotDir}/{x}").ToArray());
            Zip.ComperssToPath(hotfixFileName, "*.dll", SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget));
            ResourcePackageListManifest packageListManifest = ResourcePackageListManifest.LoadOrCreate(packageSeting.title);
            packageListManifest.CreateOrUpdatePackageManifest(aotFileName, false, Crc32.GetCRC32File(GameFrameworkEntry.GetPlatformOutputPath(aotFileName)), new string[] { aotFileName }, new string[0]);
            packageListManifest.CreateOrUpdatePackageManifest(hotfixFileName, false, Crc32.GetCRC32File(GameFrameworkEntry.GetPlatformOutputPath(hotfixFileName)), new string[] { hotfixFileName }, new string[0]);
            ResourcePackageListManifest.Save(packageListManifest);

            UploadResourcePackageCommand.Executer(OSSConfig.instance.current, GameFrameworkEntry.GetPlatformOutputPath(aotFileName));
            UploadResourcePackageCommand.Executer(OSSConfig.instance.current, GameFrameworkEntry.GetPlatformOutputPath(hotfixFileName));
            UploadResourcePackageCommand.Executer(OSSConfig.instance.current, GameFrameworkEntry.GetPlatformOutputPath($"{packageSeting.title}.ini"));
        }
    }
}
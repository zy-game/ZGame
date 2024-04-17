using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.LinkerEditor;
using ZGame.VFS;
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
            Zip.CompressToPath(aotFileName, "*.dll", aotList.Select(x => $"{aotDir}/{x}").ToArray());
            Zip.CompressToPath(hotfixFileName, "*.dll", SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget));
            ResourcePackageListManifest packageListManifest = ResourcePackageListManifest.LoadOrCreate(packageSeting.title);
            packageListManifest.CreateOrUpdatePackageManifest(aotFileName, false, Crc32.GetCRC32File(CoreAPI.GetPlatformOutputPath(aotFileName)), new string[] { aotFileName }, new string[0]);
            packageListManifest.CreateOrUpdatePackageManifest(hotfixFileName, false, Crc32.GetCRC32File(CoreAPI.GetPlatformOutputPath(hotfixFileName)), new string[] { hotfixFileName }, new string[0]);
            ResourcePackageListManifest.Save(packageListManifest);

            UploadResourcePackageCommand.Executer(ResConfig.instance.current, CoreAPI.GetPlatformOutputPath(aotFileName));
            UploadResourcePackageCommand.Executer(ResConfig.instance.current, CoreAPI.GetPlatformOutputPath(hotfixFileName));
            UploadResourcePackageCommand.Executer(ResConfig.instance.current, CoreAPI.GetPlatformOutputPath($"{packageSeting.title}.ini"));
        }
    }
}
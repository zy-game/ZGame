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
using ZGame.Resource;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public class BuildHotfixLibraryProceure : IProcedure
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">oss,packets[]</param>
        public object Execute(params object[] args)
        {
            SubGameOptions options = (SubGameOptions)args[1];
            if (options.mode is not CodeMode.Hotfix)
            {
                return default;
            }

            StripAOTDllCommand.GenerateStripedAOTDlls();
            CompileDllCommand.CompileDllActiveBuildTarget();

            IReadOnlyList<string> aotList = AppDomain.CurrentDomain.GetStaticFieldValue<IReadOnlyList<string>>("AOTGenericReferences", "PatchedAOTAssemblyList");
            if (aotList is null || aotList.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "AOT 资源编译失败！", "OK");
                return default;
            }

            ResourcePackageListManifest mainPackageManifest = PackageManifestManager.GetPackageManifest(options.mainPackageName);
            string aotFileName = $"{Path.GetFileNameWithoutExtension(options.path).ToLower()}_aot.bytes";
            string hotfixFileName = $"{Path.GetFileNameWithoutExtension(options.path).ToLower()}_hotfix.bytes";
            string aotDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);
            Zip.CompressFiles(aotFileName, aotList.Select(x => $"{aotDir}/{x}").ToArray());
            Zip.CompressFiles(hotfixFileName, SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget));
            mainPackageManifest.CreateOrUpdatePackageManifest(aotFileName, Crc32.GetCRC32Str(DateTime.Now.ToString()), new string[] { aotFileName }, new string[0]);
            mainPackageManifest.CreateOrUpdatePackageManifest(hotfixFileName, Crc32.GetCRC32Str(DateTime.Now.ToString()), new string[] { hotfixFileName }, new string[0]);
            PackageManifestManager.Save(mainPackageManifest);
            return default;
        }

        public void Release()
        {
        }
    }
}
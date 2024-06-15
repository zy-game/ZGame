using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.Resource;

namespace ZGame.Editor
{
    public class BuildResourcePackageProceure : IProcedure
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">oss,packages</param>
        public object Execute(params object[] args)
        {
            ResourceServerOptions oss = (ResourceServerOptions)args[0];
            ResourcePackageListManifest packageManifest = (ResourcePackageListManifest)args[1];
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            var manifest = BuildPipeline.BuildAssetBundles(AppCore.GetOutputPathDir(), GetAssetBundleBuildList(packageManifest).ToArray(), BuildAssetBundleOptions.None, target);
            BuildZipBundle(packageManifest);
            RefreshPackageManifestData(manifest, packageManifest);
            UploadBuildData(oss, packageManifest);
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("提示", $"打包【{packageManifest.name}】完成！！！", "确定");
            return default;
        }

        private void BuildZipBundle(ResourcePackageListManifest packageManifest)
        {
            var packageList = packageManifest.packages.Where(x => x.type == BuildType.Bytes);
            if (packageList is not null && packageList.Count() > 0)
            {
                foreach (var VARIABLE in packageList)
                {
                    EditorUtility.DisplayProgressBar("Compress", $"Compress {VARIABLE.name}", 0);
                    Zip.CompressFiles(VARIABLE.name, VARIABLE.files);
                    EditorUtility.DisplayProgressBar("Compress", $"Compress {VARIABLE.name}", 1);
                }
            }
        }

        private void UploadBuildData(ResourceServerOptions oss, ResourcePackageListManifest packageManifest)
        {
            if (packageManifest is null)
            {
                return;
            }

            foreach (var VARIABLE in packageManifest.packages)
            {
                UploadResourcePackageCommand.Executer(oss, AppCore.GetFileOutputPath(VARIABLE.name));
            }

            UploadResourcePackageCommand.Executer(oss, AppCore.GetFileOutputPath(packageManifest.name + ".ini"));
            if (packageManifest.dependencies is not null && packageManifest.dependencies.Count > 0)
            {
                foreach (var VARIABLE in packageManifest.dependencies)
                {
                    UploadBuildData(oss, PackageManifestManager.GetPackageManifest(VARIABLE));
                }
            }
        }

        private AssetBundleBuild[] GetAssetBundleBuildList(ResourcePackageListManifest packageManifest)
        {
            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();

            buildList.AddRange(packageManifest.packages.Where(x => x.type == BuildType.Bundle).Select(x => new AssetBundleBuild()
            {
                assetBundleName = x.name,
                assetNames = x.files
            }));

            if (packageManifest.dependencies is not null && packageManifest.dependencies.Count > 0)
            {
                foreach (var VARIABLE in packageManifest.dependencies)
                {
                    buildList.AddRange(GetAssetBundleBuildList(PackageManifestManager.GetPackageManifest(VARIABLE)));
                }
            }

            return buildList.ToArray();
        }

        private static void RefreshPackageManifestData(AssetBundleManifest manifest, ResourcePackageListManifest packageList)
        {
            packageList.version = Crc32.GetCRC32Str(DateTime.Now.ToString("g"));
            packageList.appVersion = Application.version;
            foreach (var VARIABLE in packageList.packages)
            {
                if (VARIABLE.type == BuildType.Bytes)
                {
                    VARIABLE.version = Crc32.GetCRC32Str(DateTime.Now.ToString("g"));
                }
                else
                {
                    BuildPipeline.GetCRCForAssetBundle(AppCore.GetFileOutputPath(VARIABLE.name), out var crc);
                    VARIABLE.dependencies = manifest.GetAllDependencies(VARIABLE.name);
                    VARIABLE.version = crc;
                }
            }

            PackageManifestManager.Save(packageList);
            if (packageList.dependencies is not null && packageList.dependencies.Count > 0)
            {
                foreach (var VARIABLE in packageList.dependencies)
                {
                    RefreshPackageManifestData(manifest, PackageManifestManager.GetPackageManifest(VARIABLE));
                }
            }
        }

        public void Release()
        {
        }
    }
}
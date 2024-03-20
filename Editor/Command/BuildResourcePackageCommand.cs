using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource;
using ZGame.Resource.Config;

namespace ZGame.Editor.Command
{
    public class BuildResourcePackageCommand
    {
        public static void Executer(params object[] args)
        {
            using (ResourcePackageCollector collector = new ResourcePackageCollector())
            {
                PackageBuilderOptions[] options = collector.OnStartCollect(args);
                List<AssetBundleBuild> list = new List<AssetBundleBuild>();
                foreach (var VARIABLE in options)
                {
                    list.AddRange(VARIABLE.builds);
                }

                BuildTarget target = BuilderConfig.instance.useActiveTarget ? EditorUserBuildSettings.activeBuildTarget : BuilderConfig.instance.target;
                Debug.Log("Build Resource Package List:" + string.Join("\n", list.Select(x => x.assetBundleName)));
                var manifest = BuildPipeline.BuildAssetBundles(GameFrameworkEntry.GetPlatformOutputPathDir(), list.ToArray(), BuildAssetBundleOptions.None, target);
                OnUploadResourcePackageList(CreatePackageManifest(manifest, options));
            }
        }

        private static List<ResourcePackageListManifest> CreatePackageManifest(AssetBundleManifest manifest, params PackageBuilderOptions[] builds)
        {
            BuildPipeline.GetCRCForAssetBundle(new DirectoryInfo(GameFrameworkEntry.GetPlatformOutputPathDir()).Name, out uint crc);
            List<ResourcePackageListManifest> packageListManifests = new List<ResourcePackageListManifest>();
            foreach (var VARIABLE in builds)
            {
                ResourcePackageListManifest packageListManifest = new ResourcePackageListManifest();
                packageListManifest.name = VARIABLE.seting.title;
                packageListManifest.packages = new ResourcePackageManifest[VARIABLE.builds.Length];
                packageListManifest.version = Crc32.GetCRC32Str(DateTime.Now.ToString("g"));
                packageListManifest.appVersion = GameConfig.instance.version;
                for (int i = 0; i < VARIABLE.builds.Length; i++)
                {
                    string[] dependencies = manifest.GetAllDependencies(VARIABLE.builds[i].assetBundleName);
                    BuildPipeline.GetCRCForAssetBundle(GameFrameworkEntry.GetPlatformOutputPath(VARIABLE.builds[i].assetBundleName), out crc);
                    packageListManifest.packages[i] = new ResourcePackageManifest()
                    {
                        name = VARIABLE.builds[i].assetBundleName.ToLower(),
                        version = crc,
                        owner = packageListManifest.name,
                        files = VARIABLE.builds[i].assetNames.Select(x => x.Replace("\\", "/")).ToArray(),
                        dependencies = dependencies,
                    };
                }

                packageListManifests.Add(packageListManifest);
            }

            foreach (var VARIABLE in packageListManifests)
            {
                File.WriteAllText(GameFrameworkEntry.GetPlatformOutputPath($"{VARIABLE.name.ToLower()}.ini"), JsonConvert.SerializeObject(VARIABLE));
            }

            return packageListManifests;
        }

        private static void OnUploadResourcePackageList(List<ResourcePackageListManifest> manifests)
        {
            int count = manifests.Count;
            int allCount = manifests.Sum(x => x.packages.Length * count);
            int successCount = 0;
            foreach (var options in manifests)
            {
                OnUploadBuilderResourcePackage(options, allCount, ref successCount);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void OnUploadBuilderResourcePackage(ResourcePackageListManifest manifest, int allCount, ref int successCount)
        {
            PackageSeting packageSeting = BuilderConfig.instance.packages.Find(x => x.title == manifest.name);
            if (packageSeting is null)
            {
                return;
            }

            foreach (var bundle in manifest.packages)
            {
                successCount++;
                string scrPath = GameFrameworkEntry.GetPlatformOutputPath(bundle.name);
                UploadResourcePackageCommand.Executer(OSSConfig.instance.current, scrPath);
                EditorUtility.DisplayProgressBar("上传进度", scrPath, successCount / (float)allCount);
            }

            UploadResourcePackageCommand.Executer(OSSConfig.instance.current, GameFrameworkEntry.GetPlatformOutputPath(manifest.name + ".ini"));
        }
    }
}
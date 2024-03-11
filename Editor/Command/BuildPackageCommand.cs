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
    public class BuildPackageCommand
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
                string output = BasicConfig.GetPlatformOutputPath(BuilderConfig.output);
                if (Directory.Exists(output) is false)
                {
                    Directory.CreateDirectory(output);
                }

                Debug.Log("Build Resource Package List:" + string.Join("\n", list.Select(x => x.assetBundleName)));
                var manifest = BuildPipeline.BuildAssetBundles(output, list.ToArray(), BuildAssetBundleOptions.None, target);
                OnUploadResourcePackageList(output, CreatePackageManifest(output, manifest, options), options);
            }
        }

        private static List<ResourcePackageListManifest> CreatePackageManifest(string output, AssetBundleManifest manifest, params PackageBuilderOptions[] builds)
        {
            BuildPipeline.GetCRCForAssetBundle(new DirectoryInfo(output).Name, out uint crc);
            List<ResourcePackageListManifest> packageListManifests = new List<ResourcePackageListManifest>();
            foreach (var VARIABLE in builds)
            {
                ResourcePackageListManifest packageListManifest = new ResourcePackageListManifest();
                packageListManifest.name = VARIABLE.seting.name;
                packageListManifest.packages = new ResourcePackageManifest[VARIABLE.builds.Length];
                packageListManifest.version = Crc32.GetCRC32Str(DateTime.Now.ToString("g"));
                packageListManifest.appVersion = BasicConfig.instance.curGame.version;
                for (int i = 0; i < VARIABLE.builds.Length; i++)
                {
                    string[] dependencies = manifest.GetAllDependencies(VARIABLE.builds[i].assetBundleName);
                    BuildPipeline.GetCRCForAssetBundle(output + "/" + VARIABLE.builds[i].assetBundleName, out crc);
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
                File.WriteAllText($"{output}/{VARIABLE.name.ToLower()}.ini", JsonConvert.SerializeObject(VARIABLE));
            }

            return packageListManifests;
        }

        private static void OnUploadResourcePackageList(string output, List<ResourcePackageListManifest> manifests, params PackageBuilderOptions[] builds)
        {
            int allCount = builds.Sum(x => x.builds.Length * x.seting.service.Selected.Length);
            int successCount = 0;
            foreach (var options in builds)
            {
                foreach (var VARIABLE in options.seting.service.Selected)
                {
                    OSSOptions oss = OSSConfig.instance.ossList.Find(x => x.title == VARIABLE);
                    if (oss is null)
                    {
                        continue;
                    }

                    foreach (var bundle in options.builds)
                    {
                        successCount++;
                        string dest = output + "/" + bundle.assetBundleName;
                        UploadPackageCommand.Executer(oss, dest);
                        EditorUtility.DisplayProgressBar("上传进度", dest, successCount / (float)allCount);
                    }

                    foreach (ResourcePackageListManifest manifest in manifests)
                    {
                        UploadPackageCommand.Executer(oss, output + "/" + manifest.name + ".ini");
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource;
using ZGame.Resource.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild
{
    [SubPageSetting("资源")]
    [ReferenceScriptableObject(typeof(BuilderConfig))]
    public class ResBuilder : SubPage
    {
        private const string key = "__build config__";

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();


            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Package List", EditorStyles.boldLabel);
            GUILayout.Space(5);
            foreach (var VARIABLE in BuilderConfig.instance.packages)
            {
                if (VARIABLE.items == null || (search.IsNullOrEmpty() is false && VARIABLE.name.StartsWith(search) is false))
                {
                    continue;
                }

                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                VARIABLE.selection = GUILayout.Toggle(VARIABLE.selection, VARIABLE.name, GUILayout.Width(300));
                GUILayout.Space(40);
                GUILayout.Label(VARIABLE.describe);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    EditorManager.SwitchScene<ResPackageSetting>();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    OnBuildBundle(new BuilderOptions[1]
                    {
                        new BuilderOptions(VARIABLE)
                    });
                }

                GUILayout.Space(2);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            if (GUILayout.Button("构建资源"))
            {
                OnBuild();
            }

            if (EditorGUI.EndChangeCheck())
            {
                BuilderConfig.OnSave();
            }
        }

        private void OnBuild()
        {
            List<BuilderOptions> builds = new List<BuilderOptions>();
            foreach (var VARIABLE in BuilderConfig.instance.packages)
            {
                if (VARIABLE.selection is false)
                {
                    continue;
                }

                builds.Add(new BuilderOptions(VARIABLE));
            }

            OnBuildBundle(builds.ToArray());
        }

        private void OnBuildBundle(params BuilderOptions[] builds)
        {
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            foreach (var VARIABLE in builds)
            {
                list.AddRange(VARIABLE.builds);
            }

            BuildTarget target = BuilderConfig.instance.useActiveTarget ? EditorUserBuildSettings.activeBuildTarget : BuilderConfig.instance.target;
            string output = BasicConfig.GetPlatformOutputPath(BuilderConfig.output);
            if (Directory.Exists(output) is false)
            {
                Directory.CreateDirectory(output);
            }

            var manifest = BuildPipeline.BuildAssetBundles(output, list.ToArray(), BuildAssetBundleOptions.None, target);
            OnUploadResourcePackageList(output, CreatePackageManifest(output, manifest, builds), builds);
            EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
        }

        private List<ResourcePackageListManifest> CreatePackageManifest(string output, AssetBundleManifest manifest, params BuilderOptions[] builds)
        {
            BuildPipeline.GetCRCForAssetBundle(new DirectoryInfo(output).Name, out uint crc);
            List<ResourcePackageListManifest> packageListManifests = new List<ResourcePackageListManifest>();
            foreach (var VARIABLE in builds)
            {
                ResourcePackageListManifest packageListManifest = new ResourcePackageListManifest();
                packageListManifest.name = VARIABLE.seting.name;
                packageListManifest.packages = new ResourcePackageManifest[VARIABLE.builds.Length];
                packageListManifest.version = Crc32.GetCRC32Str(DateTime.Now.ToString("g"));
                for (int i = 0; i < VARIABLE.builds.Length; i++)
                {
                    string[] dependencies = manifest.GetAllDependencies(VARIABLE.builds[i].assetBundleName);
                    BuildPipeline.GetCRCForAssetBundle(output + "/" + VARIABLE.builds[i].assetBundleName, out crc);
                    packageListManifest.packages[i] = new ResourcePackageManifest()
                    {
                        name = VARIABLE.builds[i].assetBundleName.ToLower(),
                        version = crc,
                        owner = packageListManifest.name,
                        files = VARIABLE.builds[i].assetNames.Select(x => x.Replace("\\", "/")).ToArray()
                    };
                }

                packageListManifests.Add(packageListManifest);
            }

            foreach (var VARIABLE in packageListManifests)
            {
                File.WriteAllText($"{output}/{VARIABLE.name}.ini", JsonConvert.SerializeObject(VARIABLE));
            }

            return packageListManifests;
        }

        private void OnUploadResourcePackageList(string output, List<ResourcePackageListManifest> manifests, params BuilderOptions[] builds)
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
                        oss.Upload(output + "/" + bundle.assetBundleName);
                        successCount++;
                        EditorUtility.DisplayProgressBar("上传进度", successCount + "/" + allCount, successCount / (float)allCount);
                    }

                    foreach (ResourcePackageListManifest manifest in manifests)
                    {
                        oss.Upload(output + "/" + manifest.name + ".ini");
                    }
                }
            }


            EditorUtility.ClearProgressBar();
        }
    }
}
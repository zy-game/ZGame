using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild
{
    [BindScene("资源")]
    [SettingContent(typeof(BuilderConfig))]
    public class ResBuilder : PageScene
    {
        private const string key = "__build config__";


        public override void OnEnable()
        {
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();


            GUILayout.BeginVertical("Package List", EditorStyles.helpBox);
            GUILayout.Space(20);
            foreach (var VARIABLE in BuilderConfig.instance.packages)
            {
                if (VARIABLE.items == null || (search.IsNullOrEmpty() is false && VARIABLE.name.StartsWith(search) is false))
                {
                    continue;
                }

                GUILayout.BeginHorizontal(ZStyle.GUI_STYLE_BOX_BACKGROUND);
                VARIABLE.selection = GUILayout.Toggle(VARIABLE.selection, VARIABLE.name, GUILayout.Width(300));
                GUILayout.Space(40);
                GUILayout.Label(VARIABLE.describe);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Edit", EditorStyles.toolbarButton))
                {
                    EditorManager.SwitchScene<ResPackageSeting>();
                }

                if (GUILayout.Button("Build", EditorStyles.toolbarButton))
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
                BuilderConfig.Saved();
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
            string output = target switch
            {
                BuildTarget.StandaloneWindows64 => BuilderConfig.output + "windows",
                BuildTarget.Android => BuilderConfig.output + "android",
                BuildTarget.WebGL => BuilderConfig.output + "webgl",
                BuildTarget.iOS => BuilderConfig.output + "ios",
                _ => BuilderConfig.output + "none"
            };

            if (Directory.Exists(output) is false)
            {
                Directory.CreateDirectory(output);
            }

            var manifest = BuildPipeline.BuildAssetBundles(output, list.ToArray(), BuildAssetBundleOptions.None, target);
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
                        dependencies = new Dependencies[dependencies.Length],
                        files = VARIABLE.builds[i].assetNames.Select(x => x.Replace("\\", "/")).ToArray()
                    };
                    for (int j = 0; j < packageListManifest.packages[i].dependencies.Length; j++)
                    {
                        BuildPipeline.GetCRCForAssetBundle(output + "/" + dependencies[j], out crc);
                        packageListManifest.packages[i].dependencies[j] = new Dependencies()
                        {
                            name = dependencies[j],
                            version = crc
                        };
                    }
                }

                packageListManifests.Add(packageListManifest);
            }

            foreach (var VARIABLE in packageListManifests)
            {
                for (int i = 0; i < VARIABLE.packages.Length; i++)
                {
                    for (int j = 0; j < VARIABLE.packages[i].dependencies.Length; j++)
                    {
                        var result = packageListManifests.Find(x => x.Contains(VARIABLE.packages[i].dependencies[j].name));
                        VARIABLE.packages[i].dependencies[j].owner = result.name;
                    }
                }

                File.WriteAllText($"{output}/{VARIABLE.name}.ini", JsonConvert.SerializeObject(VARIABLE));
            }

            EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
        }
    }
}
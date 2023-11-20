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
    [Options(typeof(BuilderConfig))]
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
                if (VARIABLE.use is false || VARIABLE.folder == null || (search.IsNullOrEmpty() is false && VARIABLE.name.StartsWith(search) is false))
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                VARIABLE.selection = GUILayout.Toggle(VARIABLE.selection, VARIABLE.name, GUILayout.Width(300));
                GUILayout.Space(40);
                GUILayout.Label(VARIABLE.describe);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Edit"))
                {
                    WindowDocker.SwitchScene<ResPackageSeting>();
                }

                GUILayout.EndHorizontal();
                GUILayout.Box("", ZStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(position.width), GUILayout.Height(1));
                GUILayout.Space(3);
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
            List<PackageSeting> items = BuilderConfig.instance.packages.Where(x => x.selection).ToList();
            if (items is null || items.Count == 0)
            {
                return;
            }

            List<BuildItem> builds = new List<BuildItem>();
            foreach (var VARIABLE in BuilderConfig.instance.packages)
            {
                if (VARIABLE.use is false)
                {
                    continue;
                }

                builds.Add(GetRuleBuildBundles(VARIABLE));
            }


            OnBuildBundle(builds.ToArray());
        }

        private string GetBundleName(PackageSeting ruler, string name)
        {
            return ruler.folder.name + "_" + Path.GetFileNameWithoutExtension(name) + BuilderConfig.instance.fileExtension;
        }

        private BuildItem GetRuleBuildBundles(PackageSeting ruler)
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            string[] files = default;
            string rootPath = AssetDatabase.GetAssetPath(ruler.folder);
            switch (ruler.buildType)
            {
                case BuildType.Asset:
                    files = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
                    foreach (var VARIABLE in files)
                    {
                        if (VARIABLE.EndsWith(".meta"))
                        {
                            continue;
                        }

                        builds.Add(new AssetBundleBuild()
                        {
                            assetBundleName = GetBundleName(ruler, VARIABLE),
                            assetNames = new[] { VARIABLE }
                        });
                    }

                    break;
                case BuildType.Folder:
                    string[] folders = Directory.GetDirectories(rootPath);
                    foreach (var folder in folders)
                    {
                        files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(".meta") is false).ToArray();
                        builds.Add(new AssetBundleBuild()
                        {
                            assetBundleName = GetBundleName(ruler, folder),
                            assetNames = files
                        });
                    }

                    break;
                case BuildType.Once:
                    builds.Add(new AssetBundleBuild()
                    {
                        assetBundleName = ruler.folder.name + "_" + Path.GetFileNameWithoutExtension(ruler.folder.name),
                        assetNames = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(".meta") is false).ToArray()
                    });
                    break;
                case BuildType.AssetType:
                    foreach (var VARIABLE in ruler.contentExtensionList)
                    {
                        files = Directory.GetFiles(rootPath, "*" + VARIABLE, SearchOption.AllDirectories).Where(x => x.EndsWith(".meta") is false).ToArray();
                        builds.Add(new AssetBundleBuild()
                        {
                            assetBundleName = GetBundleName(ruler, VARIABLE.Substring(1)),
                            assetNames = files
                        });
                    }

                    break;
            }

            return new BuildItem()
            {
                ruler = ruler,
                builds = builds.ToArray()
            };
        }

        private void OnBuildBundle(params BuildItem[] builds)
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

            var manifest = BuildPipeline.BuildAssetBundles(output, list.ToArray(), BuildAssetBundleOptions.None, target);
            Debug.Log(new DirectoryInfo(output).Name);
            BuildPipeline.GetCRCForAssetBundle(new DirectoryInfo(output).Name, out uint crc);

            foreach (var VARIABLE in builds)
            {
                ResourcePackageListManifest packageListManifest = new ResourcePackageListManifest();
                packageListManifest.name = VARIABLE.ruler.folder.name;
                packageListManifest.packages = new ResourcePackageManifest[VARIABLE.builds.Length];
                packageListManifest.version = Crc32.GetCRC32Str(DateTime.Now.ToString("g"));

                for (int i = 0; i < VARIABLE.builds.Length; i++)
                {
                    string[] dependencies = manifest.GetAllDependencies(VARIABLE.builds[i].assetBundleName);
                    BuildPipeline.GetCRCForAssetBundle(output + "/" + VARIABLE.builds[i].assetBundleName, out crc);
                    packageListManifest.packages[i] = new ResourcePackageManifest()
                    {
                        name = VARIABLE.builds[i].assetBundleName,
                        version = crc,
                        dependencies = new Dependencies[dependencies.Length],
                        files = VARIABLE.builds[i].assetNames
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

                File.WriteAllText($"{output}/{VARIABLE.ruler.folder.name}.ini", JsonConvert.SerializeObject(packageListManifest));
            }

            EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
        }

        class BuildItem
        {
            public PackageSeting ruler;
            public AssetBundleBuild[] builds;
        }
    }
}
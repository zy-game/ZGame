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

        public BuilderConfig config;

        public override void OnEnable(params object[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            if (args[0] is BuilderConfig builderConfig)
            {
                config = builderConfig;
                EditorPrefs.SetString(key, AssetDatabase.GetAssetPath(config));
            }
        }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            config = (BuilderConfig)EditorGUILayout.ObjectField("资源配置", config, typeof(BuilderConfig), false, GUILayout.MinWidth(400));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create"))
            {
                config = BuilderConfig.Create();
            }


            GUILayout.EndHorizontal();
            if (config == null)
            {
                return;
            }

            OnDrawingConfigSeting();
        }

        private void OnDrawingConfigSeting()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("Output Seting", EditorStyles.helpBox);
            GUILayout.Space(20);
            config.comperss = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("压缩方式", config.comperss);
            config.useActiveTarget = EditorGUILayout.Toggle("是否跟随激活平台", config.useActiveTarget);
            EditorGUI.BeginDisabledGroup(config.useActiveTarget);
            config.target = (BuildTarget)EditorGUILayout.EnumPopup("编译平台", config.target);
            EditorGUI.EndDisabledGroup();
            config.output = EditorGUILayout.TextField("输出路径", config.output);
            config.ex = EditorGUILayout.TextField("文件扩展名", config.ex);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("Ruler List", EditorStyles.helpBox);
            GUILayout.Space(20);
            foreach (var VARIABLE in config.ruleSeting.rulers)
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
                    WindowDocker.SwitchScene<ResRuleSeting>();
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
                EditorPrefs.SetString(key, AssetDatabase.GetAssetPath(config));
            }
        }

        private void OnBuild()
        {
            List<RulerInfoItem> items = config.ruleSeting.rulers.Where(x => x.selection).ToList();
            if (items is null || items.Count == 0)
            {
                return;
            }

            List<BuildItem> builds = new List<BuildItem>();
            foreach (var VARIABLE in config.ruleSeting.rulers)
            {
                if (VARIABLE.use is false)
                {
                    continue;
                }

                builds.Add(GetRuleBuildBundles(VARIABLE));
            }

            OnBuildBundle(config.output, config.useActiveTarget ? EditorUserBuildSettings.activeBuildTarget : config.target, builds.ToArray());
        }

        private string GetBundleName(RulerInfoItem ruler, string name)
        {
            return ruler.folder.name + "_" + Path.GetFileNameWithoutExtension(name) + config.ex;
        }

        private BuildItem GetRuleBuildBundles(RulerInfoItem ruler)
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            string[] files = default;
            string rootPath = AssetDatabase.GetAssetPath(ruler.folder);
            switch (ruler.spiltPackageType)
            {
                case SpiltPackageType.Asset:
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
                case SpiltPackageType.Folder:
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
                case SpiltPackageType.Once:
                    builds.Add(new AssetBundleBuild()
                    {
                        assetBundleName = ruler.folder.name + "_" + Path.GetFileNameWithoutExtension(ruler.folder.name),
                        assetNames = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(".meta") is false).ToArray()
                    });
                    break;
                case SpiltPackageType.AssetType:
                    foreach (var VARIABLE in ruler.exList)
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

        private void OnBuildBundle(string output, BuildTarget target, params BuildItem[] builds)
        {
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            foreach (var VARIABLE in builds)
            {
                list.AddRange(VARIABLE.builds);
            }

            var manifest = BuildPipeline.BuildAssetBundles(output, list.ToArray(), BuildAssetBundleOptions.None, target);
            Debug.Log(new DirectoryInfo(output).Name);
            BuildPipeline.GetCRCForAssetBundle(new DirectoryInfo(output).Name, out uint crc);

            foreach (var VARIABLE in builds)
            {
                BuilderManifest builderManifest = new BuilderManifest();
                builderManifest.name = VARIABLE.ruler.folder.name;
                builderManifest.packages = new PackageManifest[VARIABLE.builds.Length];
                builderManifest.version = Crc32.GetCRC32Str(DateTime.Now.ToString("g"));

                for (int i = 0; i < VARIABLE.builds.Length; i++)
                {
                    string[] dependencies = manifest.GetAllDependencies(VARIABLE.builds[i].assetBundleName);
                    BuildPipeline.GetCRCForAssetBundle(output + "/" + VARIABLE.builds[i].assetBundleName, out crc);
                    builderManifest.packages[i] = new PackageManifest()
                    {
                        name = VARIABLE.builds[i].assetBundleName,
                        version = crc,
                        dependencies = new Dependencies[dependencies.Length],
                        files = VARIABLE.builds[i].assetNames
                    };
                    for (int j = 0; j < builderManifest.packages[i].dependencies.Length; j++)
                    {
                        BuildPipeline.GetCRCForAssetBundle(output + "/" + dependencies[j], out crc);
                        builderManifest.packages[i].dependencies[j] = new Dependencies()
                        {
                            name = dependencies[j],
                            version = crc
                        };
                    }
                }

                File.WriteAllText($"{output}/{VARIABLE.ruler.folder.name}.ini", JsonConvert.SerializeObject(builderManifest));
            }

            EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
        }

        class BuildItem
        {
            public RulerInfoItem ruler;
            public AssetBundleBuild[] builds;
        }
    }
}
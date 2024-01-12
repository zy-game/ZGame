using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    class BuilderOptions
    {
        public PackageSeting seting { get; }
        public AssetBundleBuild[] builds => GetRuleBuildBundles();

        public BuilderOptions(PackageSeting seting)
        {
            this.seting = seting;
        }

        private AssetBundleBuild[] GetRuleBuildBundles()
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            foreach (var rulerData in seting.items)
            {
                switch (rulerData.buildType)
                {
                    case BuildType.Asset:
                        builds.AddRange(GetAssetBundleListWithFile(rulerData));
                        break;
                    case BuildType.Folder:
                        builds.AddRange(GetAssetBundleListWithFolder(rulerData));
                        break;
                    case BuildType.Once:
                        builds.AddRange(GetOnceBundles(rulerData));
                        break;
                    case BuildType.AssetType:
                        builds.AddRange(GetBundleListWithType(rulerData));
                        break;
                }
            }

            return builds.ToArray();
        }

        private List<AssetBundleBuild> GetBundleListWithType(RulerData rulerData)
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            if (rulerData.folder == null)
            {
                return builds;
            }

            string path = AssetDatabase.GetAssetPath(rulerData.folder);
            if (Directory.Exists(path) is false)
            {
                return builds;
            }

            foreach (var VARIABLE in rulerData.selector.items)
            {
                string[] fileList = Directory.GetFiles(path, "*" + VARIABLE, SearchOption.AllDirectories).Where(x => x.EndsWith(".meta") is false).ToArray();
                builds.Add(new AssetBundleBuild()
                {
                    assetBundleName = $"{seting.name}_{rulerData.folder.name}_{VARIABLE.name.Substring(1)}{BuilderConfig.instance.fileExtension}",
                    assetNames = fileList
                });
            }

            return builds;
        }

        private List<AssetBundleBuild> GetOnceBundles(RulerData rulerData)
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            if (rulerData.folder == null)
            {
                return builds;
            }

            string path = AssetDatabase.GetAssetPath(rulerData.folder);
            if (Directory.Exists(path) is false)
            {
                return builds;
            }

            string[] fileList = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(".meta") is false).ToArray();
            builds.Add(new AssetBundleBuild()
            {
                assetBundleName = $"{seting.name}_{rulerData.folder.name}{BuilderConfig.instance.fileExtension}",
                assetNames = fileList
            });
            return builds;
        }

        private List<AssetBundleBuild> GetAssetBundleListWithFolder(RulerData rulerData)
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            if (rulerData.folder == null)
            {
                return builds;
            }

            string path = AssetDatabase.GetAssetPath(rulerData.folder);
            if (Directory.Exists(path) is false)
            {
                return builds;
            }

            string[] folderList = Directory.GetDirectories(path);
            if (folderList.Length == 0)
            {
                builds.Add(new AssetBundleBuild()
                {
                    assetBundleName = $"{seting.name}_{rulerData.folder.name}_base{BuilderConfig.instance.fileExtension}",
                    assetNames = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(".meta") is false).ToArray()
                });
            }
            else
            {
                foreach (var VARIABLE in folderList)
                {
                    builds.Add(new AssetBundleBuild()
                    {
                        assetBundleName = $"{seting.name}_{rulerData.folder.name}_{Path.GetFileName(VARIABLE)}{BuilderConfig.instance.fileExtension}",
                        assetNames = Directory.GetFiles(VARIABLE, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(".meta") is false).ToArray()
                    });
                }
            }

            return builds;
        }

        private List<AssetBundleBuild> GetAssetBundleListWithFile(RulerData rulerData)
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            if (rulerData.folder == null)
            {
                return builds;
            }

            string path = AssetDatabase.GetAssetPath(rulerData.folder);
            if (Directory.Exists(path) is false)
            {
                return builds;
            }

            string[] fileList = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(".meta") is false).ToArray();
            foreach (var VARIABLE in fileList)
            {
                builds.Add(new AssetBundleBuild()
                {
                    assetBundleName = $"{seting.name}_{rulerData.folder.name}_{Path.GetFileName(VARIABLE)}{BuilderConfig.instance.fileExtension}",
                    assetNames = new[] { VARIABLE }
                });
            }

            return builds;
        }
    }
}
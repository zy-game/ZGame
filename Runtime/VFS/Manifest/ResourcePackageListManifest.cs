using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ZGame.Resource
{
    public class ResourcePackageListManifest
    {
        /// <summary>
        /// 资源列表名
        /// </summary>
        public string name;

        /// <summary>
        /// 资源列表版本
        /// </summary>
        public uint version;

        /// <summary>
        /// 绑定的安装包版本
        /// </summary>
        public string appVersion;

        /// <summary>
        /// 引用的资源模块
        /// </summary>
        public List<string> dependencies;

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<ResourcePackageManifest> packages;

        public bool Contains(string name)
        {
            return packages.FirstOrDefault(x => x.name == name) is not null;
        }

        public bool HasAsset(string assetName)
        {
            return packages.Any(x => x.Contains(assetName));
        }

        public string GetAssetFullPath(string assetName)
        {
            string fullPath = String.Empty;
            foreach (var VARIABLE in packages)
            {
                fullPath = VARIABLE.GetAssetFullPath(assetName);
                if (fullPath.IsNullOrEmpty() is false)
                {
                    break;
                }
            }

            return fullPath;
        }

        public void CreateOrUpdatePackageManifest(string name, bool isBundle, uint version, string[] files, string[] dependencies)
        {
            ResourcePackageManifest packageManifest = packages.Find(x => x.name == name);
            if (packageManifest is null)
            {
                packages.Add(packageManifest = new ResourcePackageManifest());
            }

            packageManifest.name = name;
            packageManifest.version = version;
            packageManifest.files = files;
            packageManifest.dependencies = dependencies;
            packageManifest.owner = this.name;
            packageManifest.isBundle = isBundle;
        }

        public static ResourcePackageListManifest LoadOrCreate(string name)
        {
            ResourcePackageListManifest packageListManifest = null;
            string fileName = name + ".ini";
            if (File.Exists(GameFrameworkEntry.GetPlatformOutputPath(fileName)) is false)
            {
                packageListManifest = new ResourcePackageListManifest()
                {
                    name = name,
                    version = 0,
                    dependencies = new List<string>(),
                    packages = new List<ResourcePackageManifest>(),
                    appVersion = GameConfig.instance.version
                };
            }
            else
            {
                string json = File.ReadAllText(GameFrameworkEntry.GetPlatformOutputPath(fileName));
                packageListManifest = JsonConvert.DeserializeObject<ResourcePackageListManifest>(json);
            }

            return packageListManifest;
        }

        public static void Save(params ResourcePackageListManifest[] packageListManifest)
        {
            foreach (var VARIABLE in packageListManifest)
            {
                File.WriteAllText(GameFrameworkEntry.GetPlatformOutputPath(VARIABLE.name + ".ini"), JsonConvert.SerializeObject(VARIABLE));
            }
        }
    }
}
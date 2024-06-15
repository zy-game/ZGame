using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZGame.Resource
{
    [HideInInspector]
    public partial class ResourcePackageListManifest : ScriptableObject
    {
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
            return packages.Exists(x => x.Contains(name));
        }

        public string GetFileFullPath(string assetName)
        {
            string fullPath = String.Empty;
            foreach (var VARIABLE in packages)
            {
                if (VARIABLE.TryGetFilePath(assetName, out fullPath))
                {
                    break;
                }
            }

            return fullPath;
        }

        public bool TryGetFilePath(string assetName, out string fullPath)
        {
            foreach (var VARIABLE in packages)
            {
                if (VARIABLE.TryGetFilePath(assetName, out fullPath))
                {
                    return true;
                }
            }

            fullPath = String.Empty;
            return false;
        }

        public bool TryGetPackageManifestWithAssetName(string assetName, out ResourcePackageManifest packageManifest)
        {
            packageManifest = default;
            foreach (var VARIABLE in packages)
            {
                if (VARIABLE.Contains(assetName))
                {
                    packageManifest = VARIABLE;
                    return true;
                }
            }

            return false;
        }

        public void CreateOrUpdatePackageManifest(string bundleName, uint version, string[] files, string[] dependencies)
        {
            ResourcePackageManifest packageManifest = packages.Find(x => x.name == bundleName);
            if (packageManifest is null)
            {
                packages.Add(packageManifest = new ResourcePackageManifest());
            }

            packageManifest.name = bundleName;
            packageManifest.version = version;
            packageManifest.files = files;
            packageManifest.dependencies = dependencies;
        }
    }
}
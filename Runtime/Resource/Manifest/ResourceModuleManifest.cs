using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZEngine.Resource
{
    /// <summary>
    /// 模块数据
    /// </summary>
    [Serializable]
    public sealed class ResourceModuleManifest : IRuntimeDatableHandle
    {
        /// <summary>
        /// 模块名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 模块版本
        /// </summary>
        public int version;

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<ResourceBundleManifest> bundleList;

        public static ResourceModuleManifest Create(string name)
        {
            ResourceModuleManifest resourceModuleManifest = new ResourceModuleManifest();
            resourceModuleManifest.name = name.ToLower();
            resourceModuleManifest.bundleList = new List<ResourceBundleManifest>();
            return resourceModuleManifest;
        }

        public ResourceBundleManifest GetBundleManifest(string bundleName)
        {
            return bundleList.Find(x => x.name == bundleName);
        }

        public ResourceBundleManifest GetBundleManifestWithAsset(string assetPath)
        {
            foreach (var bundle in bundleList)
            {
                if (bundle.files.Find(x => x.path == assetPath) is not null)
                {
                    return bundle;
                }
            }

            return default;
        }

        public ResourceBundleManifest GetBundleManifestWithAssetGUID(string guid)
        {
            foreach (var bundle in bundleList)
            {
                if (bundle.files.Find(x => x.guid == guid) is not null)
                {
                    return bundle;
                }
            }

            return default;
        }

        public ResourceBundleManifest GetBundleManifestWithAssetName(string assetName)
        {
            foreach (var bundle in bundleList)
            {
                if (bundle.files.Find(x => x.name == assetName) is not null)
                {
                    return bundle;
                }
            }

            return default;
        }

        public void Dispose()
        {
        }
    }
}
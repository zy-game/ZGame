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
    public sealed class ModuleManifest
    {
        /// <summary>
        /// 模块名
        /// </summary>
        public string name;

        /// <summary>
        /// 模块版本
        /// </summary>
        public VersionOptions version;

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<BundleManifest> bundleList;

        public BundleManifest GetBundleManifest(string bundleName)
        {
            return bundleList.Find(x => x.name == bundleName);
        }

        public BundleManifest GetBundleManifestWithAsset(string assetPath)
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

        public BundleManifest GetBundleManifestWithAssetGUID(string guid)
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

        public BundleManifest GetBundleManifestWithAssetName(string assetName)
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
    }
}
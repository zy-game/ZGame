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
    public sealed class RuntimeModuleManifest
    {
        /// <summary>
        /// 模块名
        /// </summary>
        public string name;

        /// <summary>
        /// 模块版本
        /// </summary>
        public int version;

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<RuntimeBundleManifest> bundleList;

        public static RuntimeModuleManifest Create(string name)
        {
            RuntimeModuleManifest runtimeModuleManifest = new RuntimeModuleManifest();
            runtimeModuleManifest.name = name.ToLower();
            runtimeModuleManifest.bundleList = new List<RuntimeBundleManifest>();
            return runtimeModuleManifest;
        }

        public RuntimeBundleManifest GetBundleManifest(string bundleName)
        {
            return bundleList.Find(x => x.name == bundleName);
        }

        public RuntimeBundleManifest GetBundleManifestWithAsset(string assetPath)
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

        public RuntimeBundleManifest GetBundleManifestWithAssetGUID(string guid)
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

        public RuntimeBundleManifest GetBundleManifestWithAssetName(string assetName)
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
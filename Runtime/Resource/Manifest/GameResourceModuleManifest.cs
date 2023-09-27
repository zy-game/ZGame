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
    public sealed class GameResourceModuleManifest
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
        public List<GameAssetBundleManifest> bundleList;

        public static GameResourceModuleManifest Create(string name)
        {
            GameResourceModuleManifest gameResourceModuleManifest = new GameResourceModuleManifest();
            gameResourceModuleManifest.name = name.ToLower();
            gameResourceModuleManifest.bundleList = new List<GameAssetBundleManifest>();
            return gameResourceModuleManifest;
        }

        public GameAssetBundleManifest GetBundleManifest(string bundleName)
        {
            return bundleList.Find(x => x.name == bundleName);
        }

        public GameAssetBundleManifest GetBundleManifestWithAsset(string assetPath)
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

        public GameAssetBundleManifest GetBundleManifestWithAssetGUID(string guid)
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

        public GameAssetBundleManifest GetBundleManifestWithAssetName(string assetName)
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
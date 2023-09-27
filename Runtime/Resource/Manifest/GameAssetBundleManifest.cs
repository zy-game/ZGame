using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源包数据
    /// </summary>
    [Serializable]
    public sealed class GameAssetBundleManifest
    {
        /// <summary>
        /// 资源包名
        /// </summary>
        public string name;

        /// <summary>
        /// 所属模块
        /// </summary>
        public string owner;

        /// <summary>
        /// 资源包大小
        /// </summary>
        public int length;

        /// <summary>
        /// 资源包唯一码
        /// </summary>
        public uint crc;

        /// <summary>
        /// 资源包哈希
        /// </summary>
        public string hash;

        /// <summary>
        /// 版本号哦
        /// </summary>
        public int version;

        /// <summary>
        /// 打包当前资源包的Unity版本
        /// </summary>
        public string unityVersion = String.Empty;

        /// <summary>
        /// 依赖包列表
        /// </summary>
        public List<string> dependencies;

        /// <summary>
        /// 文件列表
        /// </summary>
        public List<GameAssetObjectManifest> files;

        public static GameAssetBundleManifest Create(string owner, string bundleName)
        {
            GameAssetBundleManifest gameAssetBundleManifest = new GameAssetBundleManifest();
            gameAssetBundleManifest.files = new List<GameAssetObjectManifest>();
            gameAssetBundleManifest.dependencies = new List<string>();
            gameAssetBundleManifest.owner = owner;
            gameAssetBundleManifest.name = bundleName;
            return gameAssetBundleManifest;
        }

        public static bool operator ==(GameAssetBundleManifest l, GameAssetBundleManifest r)
        {
            return l.name == r.name && l.version == r.version;
        }

        public static bool operator !=(GameAssetBundleManifest l, GameAssetBundleManifest r)
        {
            return l.name != r.name || l.version != r.version;
        }

        public override bool Equals(object obj)
        {
            if (obj is GameAssetBundleManifest target)
            {
                return target.name == name && target.owner == owner && target.version == version;
            }

            return base.Equals(obj);
        }

        public void Refersh(int manifestVersion, int count, string[] dependencies, string hash, uint u)
        {
            this.unityVersion = Application.unityVersion;
            this.dependencies = this.dependencies;
            this.version = manifestVersion;
            this.length = count;
            this.hash = hash;
            this.crc = u;
        }
    }
}
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
    public sealed class ResourceBundleManifest : IRuntimeDatableHandle
    {
        /// <summary>
        /// 资源包名
        /// </summary>
        public string name { get; set; }

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
        public List<ResourceObjectManifest> files;

        public static ResourceBundleManifest Create(string owner, string bundleName)
        {
            ResourceBundleManifest resourceBundleManifest = new ResourceBundleManifest();
            resourceBundleManifest.files = new List<ResourceObjectManifest>();
            resourceBundleManifest.dependencies = new List<string>();
            resourceBundleManifest.owner = owner;
            resourceBundleManifest.name = bundleName;
            return resourceBundleManifest;
        }

        public static bool operator ==(ResourceBundleManifest l, ResourceBundleManifest r)
        {
            return l.name == r.name && l.version == r.version;
        }

        public static bool operator !=(ResourceBundleManifest l, ResourceBundleManifest r)
        {
            return l.name != r.name || l.version != r.version;
        }

        public bool Contians(string path)
        {
            return files.Find(x => x.path == path) is not null;
        }

        public override bool Equals(object obj)
        {
            if (obj is ResourceBundleManifest target)
            {
                return target.name == name && target.owner == owner && target.version == version;
            }

            return base.Equals(obj);
        }

        public void Dispose()
        {
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
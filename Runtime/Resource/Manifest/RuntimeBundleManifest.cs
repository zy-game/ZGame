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
    public sealed class RuntimeBundleManifest
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
        public VersionOptions version;

        /// <summary>
        /// 依赖包列表
        /// </summary>
        public List<string> dependencies;

        /// <summary>
        /// 文件列表
        /// </summary>
        public List<RuntimeAssetManifest> files;

        public static bool operator ==(RuntimeBundleManifest l, RuntimeBundleManifest r)
        {
            return l.name == r.name && l.version == r.version;
        }

        public static bool operator !=(RuntimeBundleManifest l, RuntimeBundleManifest r)
        {
            return l.name != r.name || l.version != r.version;
        }

        public override bool Equals(object obj)
        {
            if (obj is RuntimeBundleManifest target)
            {
                return target.name == name && target.owner == owner && target.version == version;
            }

            return base.Equals(obj);
        }
    }
}
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
    public sealed class BundleManifest
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
        public List<AssetManifest> files;

        /// <summary>
        /// 补丁包列表
        /// </summary>
        public List<BundlePatchManifest> patchs;
    }
}
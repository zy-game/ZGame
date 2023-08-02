using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 补丁包资源
    /// </summary>
    [Serializable]
    public sealed class PatchManifest
    {
        /// <summary>
        /// 所属资源包
        /// </summary>
        public string owerBundle;

        /// <summary>
        /// 补丁版本
        /// </summary>
        public VersionOptions version;

        /// <summary>
        /// 依赖包
        /// </summary>
        public List<string> dependencies;

        /// <summary>
        /// 文件列表
        /// </summary>
        public List<AssetManifest> files;
    }
}
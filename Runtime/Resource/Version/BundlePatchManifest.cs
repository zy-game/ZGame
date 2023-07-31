using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 补丁包资源
    /// </summary>
    [Serializable]
    public sealed class BundlePatchManifest 
    {
        public string owerBundle;
        public VersionOptions version;
        public List<string> dependencies;
        public List<AssetManifest> files;
    }
}
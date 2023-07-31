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
        public string buildTag;
        public string bundleName;
        public VersionOptions version;
        public List<string> dependencies;
        public List<AssetManifest> files;
        
        
    }
}
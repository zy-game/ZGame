using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 模块数据
    /// </summary>
    [Serializable]
    public sealed class ModuleManifest
    {
        public string moduleName;
        public VersionOptions version;
        public List<BundleManifest> bundleList;
    }
}
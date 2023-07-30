using System;
using System.Collections.Generic;

namespace ZEngine.Resource
{
    [Serializable]
    public sealed class ResourceModuleOptions
    {
        public string moduleName;
        public VersionOptions version;
        public List<ResourceBundleOptions> bundleList;
    }
}
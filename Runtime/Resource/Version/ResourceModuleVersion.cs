using System;
using System.Collections.Generic;
using ZEngine.Core;

namespace ZEngine.Resource
{
    [Serializable]
    public sealed class ResourceModuleVersion : ZVersion
    {
        public string moduleName;
        public List<ResourceBundleVersion> bundleList;
    }
}
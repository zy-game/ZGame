using System;

namespace ZEngine.Options
{
    [Serializable]
    public sealed class ResourcePreloadOptions
    {
        public string moduleName;
        public VersionOptions version;
    }
}
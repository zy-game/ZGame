using System;
using ZEngine.Core;

namespace ZEngine.Options
{
    [Serializable]
    public sealed class ResourcePreloadOptions
    {
        public string moduleName;
        public ZVersion version;
    }
}
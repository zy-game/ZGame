using System;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源数据
    /// </summary>
    [Serializable]
    public sealed class AssetManifest
    {
        public string name;
        public string guid;
        public string path;
    }
}
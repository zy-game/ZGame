using System;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源数据
    /// </summary>
    [Serializable]
    public sealed class RuntimeAssetManifest
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string name;

        /// <summary>
        /// 文件GUID
        /// </summary>
        public string guid;

        /// <summary>
        /// 文件路径
        /// Assets/.../.../
        /// </summary>
        public string path;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZGame.Resource
{
    [Serializable]
    public class ResourcePackageManifest
    {
        /// <summary>
        /// 资源包名
        /// </summary>
        public string name;

        /// <summary>
        /// 资源版本号
        /// </summary>
        public uint version;

        /// <summary>
        /// 所属资源列表
        /// </summary>
        public string owner;

        /// <summary>
        /// 资源包文件列表
        /// </summary>
        public string[] files;

        /// <summary>
        /// 引用的资源包
        /// </summary>
        public string[] dependencies;

        public bool Contains(string name)
        {
            return files.Contains(name);
        }
    }
}
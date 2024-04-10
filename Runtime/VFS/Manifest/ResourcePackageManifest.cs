using System;
using System.Collections.Generic;
using System.Linq;

namespace ZGame.VFS
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
        /// 是否资源包
        /// </summary>
        public bool isBundle;

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

        public string GetAssetFullPath(string name)
        {
            if (files is null || files.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].ToLower().EndsWith(name))
                {
                    return files[i];
                }
            }

            return String.Empty;
        }
    }
}
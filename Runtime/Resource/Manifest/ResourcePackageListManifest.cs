using System;
using System.Collections.Generic;
using System.Linq;

namespace ZGame.Resource
{
    public class ResourcePackageListManifest
    {
        /// <summary>
        /// 资源列表名
        /// </summary>
        public string name;

        /// <summary>
        /// 资源列表版本
        /// </summary>
        public uint version;

        /// <summary>
        /// 绑定的安装包版本
        /// </summary>
        public string appVersion;

        /// <summary>
        /// 引用的资源模块
        /// </summary>
        public List<string> dependencies;

        /// <summary>
        /// 资源包列表
        /// </summary>
        public ResourcePackageManifest[] packages;

        public bool Contains(string name)
        {
            return packages.FirstOrDefault(x => x.name == name) is not null;
        }

        public bool HasAsset(string assetName)
        {
            return packages.Any(x => x.Contains(assetName));
        }

        public string GetAssetFullPath(string assetName)
        {
            string fullPath = String.Empty;
            foreach (var VARIABLE in packages)
            {
                fullPath = VARIABLE.GetAssetFullPath(assetName);
                if (fullPath.IsNullOrEmpty() is false)
                {
                    break;
                }
            }

            return fullPath;
        }
    }
}
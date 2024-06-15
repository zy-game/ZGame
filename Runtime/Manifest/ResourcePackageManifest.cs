using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using System.IO;
using UnityEngine.Serialization;

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
        /// 资源目录
        /// </summary>
        public string foloder;

        /// <summary>
        /// 打包方式
        /// </summary>
        public BuildType type;

        /// <summary>
        /// 打包规则
        /// </summary>
        public BuildRuler ruler;

        /// <summary>
        /// 引用的资源包
        /// </summary>
        public string[] dependencies;

        /// <summary>
        /// 资源包文件列表
        /// </summary>
        public string[] files;

        public bool Contains(string file)
        {
            return GetFilePath(file).IsNullOrEmpty() is false;
        }

        public bool TryGetFilePath(string file, out string path)
        {
            path = GetFilePath(file);
            return path.IsNullOrEmpty() is false;
        }

        public string GetFilePath(string file)
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (PathUnit.ComparessFileName(file, files[i]))
                {
                    return files[i];
                }
            }

            return string.Empty;
        }
    }
}
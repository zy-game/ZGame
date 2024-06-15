using System;
using System.IO;
using UnityEngine;

namespace ZGame.Config
{
    [HideInInspector]
    public class ResourceServerOptions : ScriptableObject
    {
        /// <summary>
        /// 服务器类型
        /// </summary>
        public OSSType type;

        /// <summary>
        /// 存储桶名称
        /// </summary>
        public string bucket;

        /// <summary>
        /// 区域
        /// </summary>
        public string region;

        /// <summary>
        /// 密匙
        /// </summary>
        public string key;

        /// <summary>
        /// 密钥
        /// </summary>
        public string password;

        /// <summary>
        /// 是否开启加速
        /// </summary>
        public bool enableAccelerate;
    }
}
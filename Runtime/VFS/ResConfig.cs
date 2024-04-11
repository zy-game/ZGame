using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using ZGame.Config;

namespace ZGame.VFS
{
    [CreateAssetMenu(menuName = "ZGame/Config/ResConfig", fileName = "ResConfig.asset", order = 0)]
    public class ResConfig : BaseConfig<ResConfig>
    {
        /// <summary>
        /// 资源模式
        /// </summary>
        [Title("基础设置")] [LabelText("资源模式")] public ResourceMode resMode;

        [LabelText("文件后缀名")] public string ex;

        /// <summary>
        /// 卸载间隔时间
        /// </summary>
        [LabelText("资源缓存时间")] [Tooltip("当缓存的资源超过该时间后自动卸载")]
        public float timeout = 60f;

        /// <summary>
        /// 默认资源模块
        /// </summary>
        [LabelText("主包")] [Tooltip("当游戏启动时自动加载该资源包"), ValueDropdown("GetPackageList")]
        public string defaultPackageName;

        /// <summary>
        /// 代理服务器
        /// </summary>
        [LabelText("代理服务器")] public string proxy;

        /// <summary>
        /// 是否启用VFS
        /// </summary>
        [Title("虚拟文件系统设置")] [LabelText("是否开启虚拟文件系统")]
        public bool enable = true;

        private const int MIN_CHUNK_SIZE = 1024 * 1024;
        private const int MAX_CHUNK_SIZE = 1024 * 1024 * 10;

        /// <summary>
        /// VFS分块大小
        /// </summary>
        [LabelText("文件块大小"), SuffixLabel("byte")]
        public int chunkSize = 1024 * 1024;

        /// <summary>
        /// VFS数量
        /// </summary>
        [LabelText("单个文件系统文件块数量")] public int chunkCount = 20;

        /// <summary>
        /// 默认资源服务器
        /// </summary>
        [Title("资源服务器设置")] [ValueDropdown("GetOSSTitleList")] [LabelText("默认服务器")]
        public string seletion;

        /// <summary>
        /// 资源服务器配置
        /// </summary>
        [LabelText("资源服务器配置"), VerticalGroup] public List<OSSOptions> ossList;
        
        

        public OSSOptions current
        {
            get { return ossList.Find(x => x.title == seletion); }
        }

        IEnumerable GetPackageList()
        {
            return BuilderConfig.instance.packages.Select(x => x.title);
        }

        private IEnumerable GetOSSTitleList()
        {
            return ossList.Select(x => x.title);
        }

        public string GetFilePath(string fileName)
        {
            if (current is null)
            {
                return String.Empty;
            }

            return current.GetFilePath(fileName);
        }
    }

    public enum OSSType
    {
        None,
        Aliyun,
        Tencent,
        Streaming,
        URL,
    }

    [Serializable]
    public class OSSOptions
    {
        [FoldoutGroup("$title"), LabelText("别称")]
        public string title;

        [FoldoutGroup("$title"), LabelText("类型")]
        public OSSType type;

        [FoldoutGroup("$title"), LabelText("存储桶名称")]
        public string bucket;

        [FoldoutGroup("$title"), LabelText("存储桶所在地域")]
        public string region;

        [FoldoutGroup("$title"), LabelText("密匙ID")]
        public string key;

        [FoldoutGroup("$title"), LabelText("密匙")]
        public string password;

        [FoldoutGroup("$title"), LabelText("是否开启加速")]
        public bool enableAccelerate;

        public string GetFilePath(string fileName)
        {
            switch (type)
            {
                case OSSType.Streaming:
                    return Path.Combine(Application.streamingAssetsPath, fileName.ToLower());
                case OSSType.Aliyun:
                    if (enableAccelerate)
                    {
                        return $"https://{bucket}.oss-accelerate.aliyuncs.com/{GameFrameworkEntry.GetPlatformName()}/{fileName.ToLower()}";
                    }

                    return $"https://{bucket}.oss-{region}.aliyuncs.com/{GameFrameworkEntry.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.Tencent:
                    if (enableAccelerate)
                    {
                        return $"https://{bucket}.cos.accelerate.myqcloud.com/{GameFrameworkEntry.GetPlatformName()}/{fileName.ToLower()}";
                    }

                    return $"https://{bucket}.cos.{region}.myqcloud.com/{GameFrameworkEntry.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.URL:
                    return $"{region}{GameFrameworkEntry.GetPlatformName()}/{fileName.Replace(" ", "_").ToLower()}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;
using ZGame.Config;
using ZGame.Resource;

namespace ZGame
{
    /// <summary>
    /// 资源模式
    /// </summary>
    public enum ResourceMode : byte
    {
        Editor,
        Simulator,
    }

    public enum OSSType
    {
        Aliyun,
        Tencent,
        Strwaming,
    }


    [Serializable]
    public class IPConfig
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title;

        /// <summary>
        /// 地址
        /// </summary>
        public string address;

        /// <summary>
        /// 端口
        /// </summary>
        public int port;

        public string GetUrl(string path)
        {
            return $"{address}:{port}{path}";
        }
    }

    [Serializable]
    public class EntryConfig
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title;

        /// <summary>
        /// 入口DLL名称
        /// </summary>
        public string entryName;

        /// <summary>
        /// assembly 路径
        /// </summary>
        public string path;

        /// <summary>
        /// 默认语言
        /// </summary>
        public LanguageDefine language = LanguageDefine.English;

        /// <summary>
        /// 云存储
        /// </summary>
        public string oss;

        /// <summary>
        /// 资源服务名称
        /// </summary>
        public string ossTitle;

        /// <summary>
        /// 默认资源模块
        /// </summary>
        public string module;

        /// <summary>
        /// 运行方式
        /// </summary>
        public ResourceMode resMode;

        /// <summary>
        /// 卸载间隔时间
        /// </summary>
        public float unloadInterval = 60f;

        /// <summary>
        /// 引用DLL
        /// </summary>
        public List<string> references;

        [NonSerialized] public AssemblyDefinitionAsset assembly;
    }

    [Serializable]
    public class VFSConfig
    {
        public bool enable = true;
        public int chunkSize = 1024 * 1024;
        public int chunkCount = 1024;
    }

    [ResourceReference("Resources/Config/GlobalConfig.asset")]
    public sealed class GlobalConfig : SingletonScriptableObject<GlobalConfig>
    {
        /// <summary>
        /// 虚拟文件系统设置
        /// </summary>
        public VFSConfig vfsConfig;

        public string curEntryName;
        public string curAddressName;

        /// <summary>
        /// 当前游戏入口配置
        /// </summary>
        public EntryConfig curEntry
        {
            get { return entries.Find(x => x.title == curEntryName); }
        }

        /// <summary>
        /// 当前游戏地址
        /// </summary>
        public IPConfig curAddress
        {
            get { return address.Find(x => x.title == curAddressName); }
        }

        /// <summary>
        /// 游戏入口列表
        /// </summary>
        public List<EntryConfig> entries;

        /// <summary>
        /// 地址列表
        /// </summary>
        public List<IPConfig> address;


        public override void OnAwake()
        {
            vfsConfig ??= new VFSConfig();
        }

        public static string GetPlatformName()
        {
#if UNITY_ANDROID
            return "android";
#elif UNITY_IPHONE
            return "ios";
#endif
            return "windows";
        }

        /// <summary>
        /// 获取文件资源地址
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetNetworkResourceUrl(string fileName)
        {
            return $"{instance.curEntry.oss}{GetPlatformName()}/{fileName}";
        }
    }
}
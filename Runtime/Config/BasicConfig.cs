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

    public enum CodeMode
    {
        Native,
        Hotfix,
    }

    public enum OSSType
    {
        None,
        Aliyun,
        Tencent,
        Streaming,
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

        [NonSerialized] public bool isOn;

        public string GetUrl(string path)
        {
            if (port == 0)
            {
                return $"{address}{path}";
            }

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
        /// 运行模式
        /// </summary>
        public CodeMode mode;

        /// <summary>
        /// 默认资源模块
        /// </summary>
        public string module;

        /// <summary>
        /// 引用DLL
        /// </summary>
        public List<string> references;

        /// <summary>
        /// 当前渠道包配置
        /// </summary>
        public string currentChannel;

        public string version;

        /// <summary>
        /// 渠道包配置
        /// </summary>
        public List<ChannelPackageOptions> channels;

        public ChannelPackageOptions currentChannelOptions
        {
            get
            {
                if (channels == null)
                {
                    return default;
                }

                return channels.Find(x => x.title == currentChannel);
            }
        }


#if UNITY_EDITOR
        [NonSerialized] public bool isShowChannels;
        [NonSerialized] public bool isShowReferences;
        [NonSerialized] public UnityEditorInternal.AssemblyDefinitionAsset assembly;
        [NonSerialized] public List<UnityEditorInternal.AssemblyDefinitionAsset> referenceAssemblyList;
#endif
    }

    [Serializable]
    public class ChannelPackageOptions
    {
        public string title;
        public string packageName;
        public Texture2D icon;
        public Sprite splash;
        public string appName;


        /// <summary>
        /// 启动参数
        /// </summary>
        public string args;
    }

    [Serializable]
    public class VFSConfig
    {
        /// <summary>
        /// 是否启用VFS
        /// </summary>
        public bool enable = true;

        /// <summary>
        /// VFS分块大小
        /// </summary>
        public int chunkSize = 1024 * 1024;

        /// <summary>
        /// VFS数量
        /// </summary>
        public int chunkCount = 1024;
    }

    [ResourceReference("Resources/BasicConfig.asset")]
    public sealed class BasicConfig : SingletonScriptableObject<BasicConfig>
    {
        /// <summary>
        /// 虚拟文件系统设置
        /// </summary>
        public VFSConfig vfsConfig;

        /// <summary>
        /// 当前选择的游戏入口名
        /// </summary>
        public string curEntryName;

        /// <summary>
        /// 当前选择的服务器地址
        /// </summary>
        public string curAddressName;

        /// <summary>
        /// 资源模式
        /// </summary>
        public ResourceMode resMode;

        /// <summary>
        /// 卸载间隔时间
        /// </summary>
        public float resTimeout = 60f;

        public string companyName;

        /// <summary>
        /// 默认语言
        /// </summary>
        public LanguageDefine language = LanguageDefine.English;

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
        public EntryConfig curEntry;

        /// <summary>
        /// 地址列表
        /// </summary>
        public List<IPConfig> address;


        public override void OnAwake()
        {
            vfsConfig ??= new VFSConfig();
            if (address is null || address.Count == 0)
            {
                address = new List<IPConfig>();
            }

            if (curEntry.referenceAssemblyList is null)
            {
                curEntry.referenceAssemblyList = new List<AssemblyDefinitionAsset>();
            }

            if (curEntry.channels is null)
            {
                curEntry.channels = new List<ChannelPackageOptions>();
            }

            if (curEntry.references is null)
            {
                curEntry.references = new List<string>();
            }
        }

        /// <summary>
        /// 获取平台名（小写）
        /// </summary>
        /// <returns></returns>
        public static string GetPlatformName()
        {
#if UNITY_ANDROID
            return "android";
#elif UNITY_IPHONE
            return "ios";
#elif UNITY_WEBGL
            return "web";
#endif
            return "windows";
        }

        public static string GetPlatformOutputPath(string output)
        {
            return $"{output}/{GetPlatformName()}";
        }


        /// <summary>
        /// 获取服务器接口
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetApiUrl(string path)
        {
            return $"{instance.curAddress.GetUrl(path)}";
        }
    }
}
using System;
using System.Collections.Generic;
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

    [Serializable]
    public class ResConfig
    {
        /// <summary>
        /// 云存储
        /// </summary>
        public string oss;

        /// <summary>
        /// 资源配置
        /// </summary>
        public string address;

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
    }

    [Serializable]
    public class GameConfig
    {
        public string dll;
        public List<string> aot;
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
        /// 资源配置
        /// </summary>
        public ResConfig resConfig;

        /// <summary>
        /// 默认子游戏配置
        /// </summary>
        public GameConfig gameConfig;

        /// <summary>
        /// 虚拟文件系统设置
        /// </summary>
        public VFSConfig vfsConfig;

        /// <summary>
        /// 默认语言
        /// </summary>
        public LanguageDefine language = LanguageDefine.English;

        protected override void OnAwake()
        {
            gameConfig ??= new GameConfig();
            resConfig ??= new ResConfig();
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
            return $"{instance.resConfig.address}{GetPlatformName()}/{fileName}";
        }
    }
}
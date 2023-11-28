using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Resource;

namespace ZGame
{
    public enum RuntimeMode : byte
    {
        Editor,
        Simulator,
    }

    [Serializable]
    public sealed class GameSeting
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool active;

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
        public RuntimeMode runtime;

        /// <summary>
        /// DLL 名称
        /// </summary>
        public string dll;

        /// <summary>
        /// 补元数据列表
        /// </summary>
        public List<string> aot;

        public static GameSeting current { get; set; }

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
            return $"{GameSeting.current.address}/{Extension.GetPlatformName()}/{fileName}";
        }
    }
}
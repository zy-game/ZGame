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
    public class EntryConfig
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool active;

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
        public RuntimeMode runtime;

        /// <summary>
        /// DLL 名称
        /// </summary>
        public string dll;

        /// <summary>
        /// 补元数据列表
        /// </summary>
        public List<string> aot;
    }


    [CreateAssetMenu(fileName = "GlobalConfig", menuName = "ZGame/Global Config", order = 0)]
    public sealed class GlobalConfig : ScriptableObject
    {
        public List<EntryConfig> entrys;
        public int parallelRunnableCount = 10;
        private static GlobalConfig _global;
        private static EntryConfig _current;

        public static EntryConfig current
        {
            get
            {
                if (_current is null)
                {
                    _current = instance.entrys.Find(x => x.active);
                }

                return _current;
            }
        }

        public static GlobalConfig instance
        {
            get
            {
                if (_global == null)
                {
                    _global = Resources.Load<GlobalConfig>("Config/GlobalConfig");
                    if (_global == null)
                    {
                        Debug.LogError("Global.asset not found!");
                    }
                }

                return _global;
            }
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
            return $"{GlobalConfig.current.address}{GetPlatformName()}/{fileName}";
        }
    }
}
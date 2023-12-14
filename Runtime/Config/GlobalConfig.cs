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


    [CreateAssetMenu(fileName = "GlobalConfig", menuName = "ZGame/Global Config", order = 0)]
    public sealed class GlobalConfig : ScriptableObject
    {
        public List<EntryConfig> entrys;
        public int parallelRunnableCount = 10;
        public float unloadBundleInterval = 60f;
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
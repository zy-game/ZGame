using System;
using UnityEditor;
using UnityEngine;
using ZEngine.Game;

namespace ZEngine
{
    public class Custom
    {
        public static void Initialize()
        {
            Console.instance.ShowConsole();
            GameManager.instance.Initialize();

            ITiming.Default = ITiming.Create(30);
        }

        /// <summary>
        /// 退出播放
        /// </summary>
        public static void Quit()
        {
            EnumeratorExtension.StopAll();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        /// <summary>
        /// 获取当前运行时平台名(小写)
        /// </summary>
        /// <returns></returns>
        public static string GetPlatfrom()
        {
#if UNITY_ANDROID
                    return "android";
#elif UNITY_IPHONE
                    return "ios";
#else
            return "windows";
#endif
        }

        /// <summary>
        /// 获取随机名
        /// </summary>
        /// <returns></returns>
        public static string RandomName()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static float Random(float l, float r)
            => UnityEngine.Random.Range(l, r);

        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static int Random(int l, int r)
            => UnityEngine.Random.Range(l, r);

        /// <summary>
        /// 获取热更资源路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetHotfixPath(string url, string name)
        {
            return $"{url}/{ZGame.GetPlatfrom()}/{name}";
        }

        /// <summary>
        /// 获取本地缓存文件路径
        /// </summary>
        /// <param name="fileName">文件名，不包含扩展名</param>
        /// <returns></returns>
        public static string GetLocalFilePath(string fileName)
        {
            return $"{Application.persistentDataPath}/{fileName}";
        }
    }
}
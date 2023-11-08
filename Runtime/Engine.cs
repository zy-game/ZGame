using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Localization;
using ZGame.Networking;
using ZGame.Resource;
using ZGame.Window;

namespace ZGame
{
    public sealed class Engine
    {
        /// <summary>
        /// 文件系统
        /// </summary>
        public static FileManager File { get; private set; }

        /// <summary>
        /// 游戏系统
        /// </summary>
        public static GameManager Game { get; private set; }

        /// <summary>
        /// 本地化
        /// </summary>
        public static LanguageManager Localization { get; private set; }

        /// <summary>
        /// 网络系统
        /// </summary>
        public static NetworkManager Network { get; private set; }

        /// <summary>
        /// 资源管理系统
        /// </summary>
        public static ResourceManager Resource { get; private set; }

        /// <summary>
        /// 界面UI系统
        /// </summary>
        public static WindowManager Window { get; private set; }

        /// <summary>
        /// 像机管理器
        /// </summary>
        public static CameraManager Cameras { get; private set; }

        /// <summary>
        /// 是否使用热更新
        /// </summary>
        public static bool IsHotfix { get; private set; }


        public async static void Initialized(GameSeting setting)
        {
            IsHotfix = setting.useHotfix;
            Network = new NetworkManager();
            File = new FileManager();
            Localization = new LanguageManager();
            Resource = new ResourceManager();
            Window = new WindowManager();
            Game = new GameManager();
            Cameras = new CameraManager();
            Resource.SetResourceAddressable(setting.resUrl);
            Localization.SwitchLanguage(setting.Language);
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

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
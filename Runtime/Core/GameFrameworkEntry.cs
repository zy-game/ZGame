using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using ZGame.Config;
using ZGame.Data;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Language;
using ZGame.Logger;
using ZGame.Networking;
using ZGame.Download;
using ZGame.Notify;
using ZGame.Resource;
using ZGame.Sound;
using ZGame.Thread;
using ZGame.UI;
using ZGame.Web;

namespace ZGame
{
    /// <summary>
    /// 框架入口
    /// </summary>
    public static class GameFrameworkEntry
    {
        private static List<GameFrameworkModule> _modules = new List<GameFrameworkModule>();

        /// <summary>
        /// 资源管理
        /// </summary>
        public static ResourceManager Resource { get; private set; }

        /// <summary>
        /// 虚拟文件系统
        /// </summary>
        public static VFSManager VFS { get; private set; }

        /// <summary>
        /// 运行时数据管理
        /// </summary>
        public static DatableManager Datable { get; private set; }

        /// <summary>
        /// 缓存数据管理
        /// </summary>
        public static CookieManager DataCache { get; private set; }

        /// <summary>
        /// 多语言管理
        /// </summary>
        public static LanguageManager Language { get; private set; }

        /// <summary>
        /// UI 管理器
        /// </summary>
        public static UIManager UI { get; private set; }

        /// <summary>
        /// Http 管理器
        /// </summary>
        public static HttpClient Web { get; private set; }

        /// <summary>
        /// 音效管理器
        /// </summary>
        public static AudioPlayerManager Audio { get; private set; }

        /// <summary>
        /// 录音管理器
        /// </summary>
        public static RecorderManager Recorder { get; private set; }

        /// <summary>
        /// 通知管理器
        /// </summary>
        public static NotifyManager Notify { get; private set; }

        /// <summary>
        /// 世界管理器
        /// </summary>
        public static WorldManager World { get; private set; }

        /// <summary>
        /// 实体管理器
        /// </summary>
        public static EntityManager Entity { get; private set; }

        /// <summary>
        /// 配置管理
        /// </summary>
        public static ConfigManager Config { get; private set; }

        /// <summary>
        /// 下载器
        /// </summary>
        public static DownloadManager Download { get; private set; }

        /// <summary>
        /// 日志管理
        /// </summary>
        public static LoggerManager Logger { get; private set; }

        /// <summary>
        /// 协程管理器
        /// </summary>
        public static CoroutineManager Coroutine { get; private set; }

        /// <summary>
        /// 键盘输入管理
        /// </summary>
        public static KeyboardManager Keyboard { get; private set; }

        /// <summary>
        /// 初始化框架
        /// </summary>
        public static async void Initialized()
        {
            VFS = GetOrCreateModule<VFSManager>();
            Datable = GetOrCreateModule<DatableManager>();
            DataCache = GetOrCreateModule<CookieManager>();
            Language = GetOrCreateModule<LanguageManager>();
            UI = GetOrCreateModule<UIManager>();
            Web = GetOrCreateModule<HttpClient>();
            Audio = GetOrCreateModule<AudioPlayerManager>();
            Recorder = GetOrCreateModule<RecorderManager>();
            Resource = GetOrCreateModule<ResourceManager>();
            World = GetOrCreateModule<WorldManager>();
            Entity = GetOrCreateModule<EntityManager>();
            Notify = GetOrCreateModule<NotifyManager>();
            Config = GetOrCreateModule<ConfigManager>();
            Download = GetOrCreateModule<DownloadManager>();
            Logger = GetOrCreateModule<LoggerManager>();
            Coroutine = GetOrCreateModule<CoroutineManager>();
            Keyboard = GetOrCreateModule<KeyboardManager>();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            GameFrameworkEntry.Language.Switch(ChannelConfigList.instance.current.language);
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("正在获取配置信息..."));
            UILoading.SetProgress(0);
            if (GameConfig.instance is null)
            {
                Debug.LogError(new EntryPointNotFoundException());
                return;
            }

            if (await GameFrameworkEntry.Resource.PreloadingResourcePackageList(ResConfig.instance.defaultPackageName) is false)
            {
                return;
            }

            await GameFrameworkEntry.Resource.LoadGameAssembly(GameConfig.instance);
        }

        /// <summary>
        /// 反初始化框架
        /// </summary>
        public static void Uninitialized()
        {
            Notify.Notify(new AppQuitEventDatable());
            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].Dispose();
            }

            _modules.Clear();
            Recorder = null;
            Resource = null;
            VFS = null;
            Datable = null;
            DataCache = null;
            Language = null;
            UI = null;
            Web = null;
            Audio = null;
            Notify = null;
            World = null;
            Entity = null;
            Config = null;
            Download = null;
            Logger = null;
            Coroutine = null;
            Keyboard = null;
        }


        internal static void FixedUpdate()
        {
            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].FixedUpdate();
            }
        }

        internal static void Update()
        {
            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].Update();
            }
        }

        internal static void LateUpdate()
        {
            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].LateUpdate();
            }
        }

        /// <summary>
        /// 获取或加载模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrCreateModule<T>() where T : GameFrameworkModule
        {
            return (T)GetOrCreateModule(typeof(T));
        }

        /// <summary>
        /// 获取或加载模块
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GameFrameworkModule GetOrCreateModule(Type type)
        {
            GameFrameworkModule frameworkModule = _modules.Find(m => m.GetType() == type);
            if (frameworkModule is null)
            {
                frameworkModule = (GameFrameworkModule)Activator.CreateInstance(type);
                frameworkModule.OnAwake();
                _modules.Add(frameworkModule);
            }

            return frameworkModule;
        }

        /// <summary>
        /// 释放模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void DisposeModule<T>() where T : GameFrameworkModule
        {
            DisposeModule(typeof(T));
        }

        /// <summary>
        /// 释放模块
        /// </summary>
        /// <param name="type"></param>
        public static void DisposeModule(Type type)
        {
            GameFrameworkModule frameworkModule = _modules.Find(m => m.GetType() == type);
            if (frameworkModule is null)
            {
                return;
            }

            frameworkModule.Dispose();
            _modules.Remove(frameworkModule);
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

        /// <summary>
        /// 获取输出路径
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static string GetPlatformOutputPath(string fileName)
        {
            string path = $"{Application.dataPath}/../output/{GetPlatformName()}";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            return $"{path}/{fileName}";
        }

        /// <summary>
        /// 获取输出路径
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static string GetPlatformOutputPathDir()
        {
            string path = $"{Application.dataPath}/../output/{GetPlatformName()}";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// 获取输出根目录
        /// </summary>
        /// <returns></returns>
        public static string GetPlatformOutputBaseDir()
        {
            string path = $"{Application.dataPath}/../output";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// 获取文件缓存路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetApplicationFilePath(string fileName)
        {
            return $"{Application.persistentDataPath}/{fileName}";
        }

        /// <summary>
        /// 获取服务器接口
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetApiUrl(string path)
        {
            return $"{IPConfig.instance.GetUrl(path)}";
        }
    }
}
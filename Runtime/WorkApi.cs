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
using ZGame.Module;
using ZGame.Networking;
using ZGame.Networking.Download;
using ZGame.Notify;
using ZGame.Resource;
using ZGame.Sound;
using ZGame.UI;

namespace ZGame
{
    /// <summary>
    /// 框架入口
    /// </summary>
    public static class WorkApi
    {
        private static List<IModule> _modules = new List<IModule>();

        /// <summary>
        /// 是否开启虚拟文件系统
        /// </summary>
        public static bool IsEnableVirtualFileSystem => BasicConfig.instance.vfsConfig.enable;

        /// <summary>
        /// 当前游戏配置
        /// </summary>
        public static GameConfig CurGame => BasicConfig.instance.curGame;

        /// <summary>
        /// 当前服务器地址
        /// </summary>
        public static IPConfig CurAddress => BasicConfig.instance.curAddress;

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
        public static RuntimeDatableManager Datable { get; private set; }

        /// <summary>
        /// 缓存数据管理
        /// </summary>
        public static DatableCacheManager DataCache { get; private set; }

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
        public static WebNet Web { get; private set; }

        /// <summary>
        /// 游戏管理器
        /// </summary>
        public static GameManager Game { get; private set; }

        /// <summary>
        /// 音效管理器
        /// </summary>
        public static SoundManager Audio { get; private set; }

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
        /// 初始化框架
        /// </summary>
        public static async void Initialized()
        {
            VFS = GetOrCreateModule<VFSManager>();
            Datable = GetOrCreateModule<RuntimeDatableManager>();
            DataCache = GetOrCreateModule<DatableCacheManager>();
            Language = GetOrCreateModule<LanguageManager>();
            UI = GetOrCreateModule<UIManager>();
            Web = GetOrCreateModule<WebNet>();
            Game = GetOrCreateModule<GameManager>();
            Audio = GetOrCreateModule<SoundManager>();
            Resource = GetOrCreateModule<ResourceManager>();
            World = GetOrCreateModule<WorldManager>();
            Entity = GetOrCreateModule<EntityManager>();
            Notify = GetOrCreateModule<NotifyManager>();
            Config = GetOrCreateModule<ConfigManager>();
            Download = GetOrCreateModule<DownloadManager>();
            Logger = GetOrCreateModule<LoggerManager>();
            Debug.Log(GameManager.DefaultWorld.name);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            WorkApi.Language.Switch(BasicConfig.instance.curGame.language);
            UILoading.SetTitle(WorkApi.Language.Query("正在获取配置信息..."));
            UILoading.SetProgress(0);
            if (BasicConfig.instance.curGame is null)
            {
                Debug.LogError(new EntryPointNotFoundException());
                return;
            }

            bool state = await WorkApi.Resource.PreloadingResourcePackageList(BasicConfig.instance.curGame);
            if (state is false)
            {
                return;
            }

            await WorkApi.Game.EntrySubGame(BasicConfig.instance.curGame);
        }

        /// <summary>
        /// 反初始化框架
        /// </summary>
        public static void Uninitialized()
        {
            Notify.Notify(new AppQuitEventArgs());
            _modules.ForEach(x => x.Dispose());
            _modules.Clear();
            Resource = null;
            VFS = null;
            Datable = null;
            DataCache = null;
            Language = null;
            UI = null;
            Web = null;
            Game = null;
            Audio = null;
            Notify = null;
            World = null;
            Entity = null;
            Config = null;
            Download = null;
            Logger = null;
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 获取或加载模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrCreateModule<T>() where T : IModule
        {
            return (T)GetOrCreateModule(typeof(T));
        }

        /// <summary>
        /// 获取或加载模块
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IModule GetOrCreateModule(Type type)
        {
            IModule module = _modules.Find(m => m.GetType() == type);
            if (module is null)
            {
                module = (IModule)Activator.CreateInstance(type);
                module.OnAwake();
                _modules.Add(module);
            }

            return module;
        }

        /// <summary>
        /// 释放模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void DisposeModule<T>() where T : IModule
        {
            DisposeModule(typeof(T));
        }

        /// <summary>
        /// 释放模块
        /// </summary>
        /// <param name="type"></param>
        public static void DisposeModule(Type type)
        {
            IModule module = _modules.Find(m => m.GetType() == type);
            if (module is null)
            {
                return;
            }

            module.Dispose();
            _modules.Remove(module);
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
            return $"{CurAddress.GetUrl(path)}";
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Config;
using ZGame.Data;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Module;
using ZGame.Networking;
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
            Debug.Log(GameManager.DefaultWorld.name);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            WorkApi.Language.Switch(BasicConfig.instance.language);
            UILoading.SetTitle(WorkApi.Language.Query("正在获取配置信息..."));
            UILoading.SetProgress(0);
            if (BasicConfig.instance.curEntry is null)
            {
                Debug.LogError(new EntryPointNotFoundException());
                return;
            }

            await WorkApi.Resource.PreloadingResourcePackageList(BasicConfig.instance.curEntry);
            await WorkApi.Game.EntrySubGame(BasicConfig.instance.curEntry);
        }

        /// <summary>
        /// 反初始化框架
        /// </summary>
        public static void Uninitialized()
        {
            foreach (IModule module in _modules)
            {
                module.Dispose();
            }

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
    }
}
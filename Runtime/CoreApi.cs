using System;
using UnityEngine;
using ZGame.Execute;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Localization;
using ZGame.Networking;
using ZGame.Resource;
using ZGame.Window;

namespace ZGame
{
    public sealed class CoreApi
    {
        /// <summary>
        /// 文件系统
        /// </summary>
        public static IFileSystem fileSystem { get; private set; }

        /// <summary>
        /// 游戏系统
        /// </summary>
        public static IGameSystem gameSystem { get; private set; }

        /// <summary>
        /// 本地化
        /// </summary>
        public static ILocalizationSystem localizationSystem { get; private set; }

        /// <summary>
        /// 网络系统
        /// </summary>
        public static INetworkManager networkSystem { get; private set; }

        /// <summary>
        /// 资源管理系统
        /// </summary>
        public static IResourceSystem resourceSystem { get; private set; }

        /// <summary>
        /// 界面UI系统
        /// </summary>
        public static IWindowSystem windowSystem { get; private set; }

        /// <summary>
        /// 执行管理器
        /// </summary>
        public static IExecuteManager executeManager { get; private set; }

        public async static void Initialized(Startup startup)
        {
            if (string.IsNullOrEmpty(startup.module))
            {
                IMsgBox.Create("加载公共配置失败", Quit);
                return;
            }

            IExecutePipeline executePipeline = AppDomain.CurrentDomain.CreateInstance<IExecutePipeline>(startup.executer);
            if (executePipeline is null)
            {
                return;
            }

            executePipeline.Execute();
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
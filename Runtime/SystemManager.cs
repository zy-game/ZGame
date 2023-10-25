using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Localization;
using ZGame.Networking;
using ZGame.Resource;
using ZGame.Window;

namespace ZGame
{
    public sealed class SystemManager
    {
        /// <summary>
        /// 文件系统
        /// </summary>
        public static IFileSystem fileSystem { get; internal set; }

        /// <summary>
        /// 游戏系统
        /// </summary>
        public static IGameSystem gameSystem { get; internal set; }

        /// <summary>
        /// 本地化
        /// </summary>
        public static ILocalizationSystem localizationSystem { get; internal set; }

        /// <summary>
        /// 网络系统
        /// </summary>
        public static INetworkSystem networkSystem { get; internal set; }

        /// <summary>
        /// 资源管理系统
        /// </summary>
        public static IResourceSystem resourceSystem { get; internal set; }

        /// <summary>
        /// 界面UI系统
        /// </summary>
        public static IWindowSystem windowSystem { get; internal set; }

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
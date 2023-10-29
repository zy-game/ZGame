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
    public sealed class CoreApi
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
        /// 资源服务器地址
        /// </summary>
        public static string ResourceAddress { get; private set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public static string ServerAddress { get; private set; }

        /// <summary>
        /// 当前语言
        /// </summary>
        public static Language CurrentLanguage { get; private set; }

        /// <summary>
        /// 是否使用热更新
        /// </summary>
        public static bool IsHotfix { get; private set; }


        public async static void Initialized(Startup startup)
        {
            IsHotfix = startup.useHotfix;
            Network = new NetworkManager();
            File = new FileManager();
            Localization = new LanguageManager();
            Localization.SwitchLanguage(CurrentLanguage);
            Resource = new ResourceManager();
            Window = new WindowManager();
            Game = new GameManager();

            GameSetting setting = startup.GameSettings.Find(x => x.active);
            if (setting is not null)
            {
                ResourceAddress = setting.resUrl;
                ServerAddress = setting.serverUrl;
                CurrentLanguage = setting.Language;
                await UpdateAndLoadingResourcePackageList(setting);
            }

            await EntryGame(setting);
        }

        private static async UniTask UpdateAndLoadingResourcePackageList(GameSetting setting)
        {
            if (setting.module.IsNullOrEmpty())
            {
                return;
            }

            GameLoadingWindow loading = Window.GeOrOpentWindow<GameLoadingWindow>();
            ErrorCode errorCode = await Resource.UpdateResourcePackageList(setting.module, loading.Setup);
            if (errorCode is not ErrorCode.OK)
            {
                return;
            }

            errorCode = await Resource.LoadingResourcePackageList(setting.module, loading.Setup);
            if (errorCode is not ErrorCode.OK)
            {
                return;
            }

            await EntryGame(setting);
        }

        private static async UniTask EntryGame(GameSetting settings)
        {
#if UNITY_EDITOR
            if (IsHotfix is false)
            {
                foreach (var VARIABLE in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (VARIABLE.GetName().Name.Equals(Path.GetFileNameWithoutExtension(settings.dll)) is false)
                    {
                        continue;
                    }

                    MethodInfo methodInfo = VARIABLE.GetType("Program")?.GetMethod("Main");
                    if (methodInfo is null)
                    {
                        Debug.LogError("未找到入口函数:Program.Main");
                        continue;
                    }

                    methodInfo.Invoke(null, new object[0]);
                }

                return;
            }
#endif
            TextAsset textAsset = Resource.LoadAsset(Path.GetFileNameWithoutExtension(settings.dll) + ".bytes")?.Require<TextAsset>();
            if (textAsset == null)
            {
                //Quit();
                return;
            }

            Assembly assembly = Assembly.Load(textAsset.bytes);

            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var item in settings.aot)
            {
                textAsset = Resource.LoadAsset(Path.GetFileNameWithoutExtension(item) + ".bytes")?.Require<TextAsset>();
                if (textAsset == null)
                {
                    Debug.LogError("加载AOT补元数据资源失败");
                    return;
                }

                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{item}. mode:{mode} ret:{err}");
            }

            MethodInfo method = assembly.GetType("Program")?.GetMethod("Main");
            if (method is null)
            {
                //Quit();
                return;
            }

            method.Invoke(null, new object[0]);
        }

        public static string GetNetworkResourceUrl(string fileName)
        {
            return $"{ResourceAddress}/{GetPlatformName()}/{fileName}";
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
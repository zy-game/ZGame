using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using ZGame.Config;
using ZGame.Data;
// using ZGame.Data;
using ZGame.Game;
using ZGame.Language;
using ZGame.Logger;
using ZGame.Networking;
using ZGame.Events;
using ZGame.Sound;
using ZGame.UI;
using ZGame.Resource;

namespace ZGame
{
    /// <summary>
    /// 框架入口
    /// </summary>
    public static class AppCore
    {
        private static List<GameManager.Behaviour> _modules = new();
        private static AppStart _start;

        /// <summary>
        /// 命令管理器
        /// </summary>
        public static ProcedureManager Procedure { get; private set; }

        /// <summary>
        /// 摄像机管理器
        /// </summary>
        public static CameraManager Camera { get; private set; }

        /// <summary>
        /// 文件管理器
        /// </summary>
        public static VFSManager File { get; private set; }

        /// <summary>
        /// 资源包清单
        /// </summary>
        public static PackageManifestManager Manifest { get; private set; }

        /// <summary>
        /// 虚拟文件系统
        /// </summary>
        public static ResourceManager Resource { get; private set; }

        /// <summary>
        /// 运行时数据管理
        /// </summary>
        public static DataManager Datable { get; private set; }
        //
        // /// <summary>
        // /// 缓存数据管理
        // /// </summary>
        // public static LocationManager Location { get; private set; }

        /// <summary>
        /// 多语言管理
        /// </summary>
        public static LanguageManager Language { get; private set; }

        /// <summary>
        /// UI 管理器
        /// </summary>
        public static UIManager UI { get; private set; }

        /// <summary>
        /// 音效管理器
        /// </summary>
        public static AudioManager Audio { get; private set; }

        /// <summary>
        /// 录音管理器
        /// </summary>
        public static RecorderManager Recorder { get; private set; }

        /// <summary>
        /// 事件管理器
        /// </summary>
        public static GameEventManager Events { get; private set; }

        /// <summary>
        /// 配置管理
        /// </summary>
        public static ConfigManager Config { get; private set; }

        /// <summary>
        /// 日志管理
        /// </summary>
        public static LoggerManager Logger { get; private set; }

        /// <summary>
        /// 网络管理
        /// </summary>
        public static NetworkManager Network { get; private set; }

        /// <summary>
        /// ECS管理器
        /// </summary>
        public static ECSManager ECS { get; private set; }

        /// <summary>
        /// 资源模式
        /// </summary>
        public static ResourceMode resMode => _start.resMode;

        /// <summary>
        /// 运行模式
        /// </summary>
        public static CodeMode mode => _start.subGame.mode;

        /// <summary>
        /// 资源服务器类型
        /// </summary>
        public static OSSType OssType => _start.ossOptions.type;

        /// <summary>
        /// APP版本
        /// </summary>
        public static string version => _start.subGame.version;

        /// <summary>
        /// 分辨率
        /// </summary>
        public static Vector2 resolution => _start.resolution;

        /// <summary>
        /// 卸载间隔时间
        /// </summary>
        public static float timeout => 60f;

        /// <summary>
        /// 是否启用日志
        /// </summary>
        public static bool isDebug => _start.isDebug;

        /// <summary>
        /// 游戏名称
        /// </summary>
        public static string name { get; private set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public static string hosting { get; private set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public static ushort port { get; private set; }

        /// <summary>
        /// 获取管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetManager<T>(params object[] args) where T : GameManager
        {
            return (T)GetManager(typeof(T), args);
        }

        /// <summary>
        /// 获取管理器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GameManager GetManager(Type type, params object[] args)
        {
            if (typeof(GameManager).IsAssignableFrom(type) is false)
            {
                throw new Exception($"{type.Name} is not a GameManager");
            }

            GameManager.Behaviour behaviour = _modules.Find(x => x.Manager.GetType() == type);
            if (behaviour is null)
            {
                behaviour = new GameObject(type.Name).AddComponent<GameManager.Behaviour>();
                behaviour.gameObject.SetParent(_start.transform, Vector3.zero, Vector3.zero, Vector3.one);
                behaviour.Manager = (GameManager)RefPooled.Alloc(type);
                _modules.Add(behaviour);
                behaviour.Manager.OnAwake(args);
            }

            return behaviour.Manager;
        }

        /// <summary>
        /// 关闭管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Close<T>() where T : GameManager
        {
            Close(typeof(T));
        }

        /// <summary>
        /// 关闭管理器
        /// </summary>
        /// <param name="type"></param>
        public static void Close(Type type)
        {
            if (typeof(GameManager).IsAssignableFrom(type) is false)
            {
                throw new Exception($"{type.Name} is not a GameManager");
            }

            GameManager.Behaviour behaviour = _modules.Find(x => x.Manager.GetType() == type);
            if (behaviour is null)
            {
                return;
            }

            _modules.Remove(behaviour);
            RefPooled.Free(behaviour.Manager);
            behaviour.Manager = null;
            GameObject.DestroyImmediate(behaviour.gameObject);
        }

        public static void Quit()
        {
            Close<RecorderManager>();
            Close<ECSManager>();
            Close<CameraManager>();
            Close<ProcedureManager>();
            Close<UIManager>();
            Close<AudioManager>();
            Close<ConfigManager>();
            Close<ResourceManager>();
            Close<NetworkManager>();
            // Close<LocationManager>();
            Close<VFSManager>();
            Close<PackageManifestManager>();
            Close<LanguageManager>();
            Close<DataManager>();
            Close<GameEventManager>();
            Close<LoggerManager>();
            for (int i = _modules.Count - 1; i >= 0; i--)
            {
                Close(_modules[i].Manager.GetType());
            }

            _modules.Clear();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 初始化框架
        /// </summary>
        public static async void Initialized(AppStart start)
        {
            _start = start;
            _start.resolution = _start.isFullScreen ? new Vector2(Screen.width, Screen.height) : _start.resolution;
            name = start.subGame.name.Replace(" Game Options");
            hosting = start.gameServerOptions?.hosting;
            port = start.gameServerOptions == null ? (ushort)0 : start.gameServerOptions.port;
            Logger = GetManager<LoggerManager>();
            Camera = GetManager<CameraManager>();
            File = GetManager<VFSManager>();
            Procedure = GetManager<ProcedureManager>();
            Manifest = GetManager<PackageManifestManager>();
            Language = GetManager<LanguageManager>();
            Datable = GetManager<DataManager>();
            Resource = GetManager<ResourceManager>();
            // Location = GetManager<LocationManager>();
            UI = GetManager<UIManager>();
            Audio = GetManager<AudioManager>();
            Recorder = GetManager<RecorderManager>();
            Events = GetManager<GameEventManager>();
            Config = GetManager<ConfigManager>();
            Network = GetManager<NetworkManager>();
            ECS = GetManager<ECSManager>();

            start.gameObject.AddComponent<CinemachineBrain>().m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Language.Switch(start.subGame.language);
            UILoading.SetTitle(Language.Query(LanguageCode.DownloadManifestInfo));
            UILoading.SetProgress(0);

            // if (_start.resMode is ResourceMode.Simulator)
            // {
            //     if (await Resource.LoadingResourcePackageAsync(start.subGame.mainPackageName) is not Status.Success)
            //     {
            //         UIMsgBox.Show(Language.Query(LanguageCode.PackageLoadingFailure), Quit);
            //         return;
            //     }
            // }

            if (await LoadSubGameAsync(start.subGame) is not Status.Success)
            {
                UIMsgBox.Show(Language.Query(LanguageCode.PackageLoadingFailure), Quit);
                return;
            }

            UILoading.Hide();
        }

        public static async UniTask<Status> LoadSubGameAsync(SubGameOptions subGame)
        {
            Assembly assembly = default;
            if (subGame.dllName.IsNullOrEmpty())
            {
                AppCore.Logger.LogError(new NullReferenceException(nameof(subGame.dllName)));
                return Status.Fail;
            }

            if (subGame.mode is CodeMode.Hotfix && AppCore.resMode == ResourceMode.Simulator)
            {
                LoadAotAssemblies(subGame);
                LoadHotfixAssembly(subGame);
            }

            assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(subGame.dllName)).FirstOrDefault();
            if (assembly is null)
            {
                UIMsgBox.Show(AppCore.Language.Query(LanguageCode.NotFindAssemblyEntryPoint), AppCore.Quit);
                return Status.Fail;
            }

            return await ECS.LoadWorld(assembly);
        }

        private static async UniTask LoadAotAssemblies(SubGameOptions subGame)
        {
            string aotFileName = $"{subGame.dllName.ToLower()}_aot.bytes";
            byte[] bytes = await AppCore.File.ReadAsync(aotFileName);
            Dictionary<string, byte[]> aotZipDict = await Zip.Decompress(bytes);
            foreach (var VARIABLE in aotZipDict)
            {
                if (RuntimeApi.LoadMetadataForAOTAssembly(VARIABLE.Value, HomologousImageMode.SuperSet) != LoadImageErrorCode.OK)
                {
                    AppCore.Logger.LogError("加载AOT补元数据资源失败:" + VARIABLE.Key);
                    continue;
                }
            }
        }

        private static async UniTask<Assembly> LoadHotfixAssembly(SubGameOptions subGame)
        {
            string hotfixFile = $"{subGame.dllName.ToLower()}_hotfix.bytes";

            var bytes = await AppCore.File.ReadAsync(hotfixFile);
            Dictionary<string, byte[]> dllZipDict = await Zip.Decompress(bytes);
            if (dllZipDict.TryGetValue(subGame.dllName + ".dll", out byte[] dllBytes) is false)
            {
                AppCore.Logger.LogError(new NullReferenceException(subGame.dllName));
                return default;
            }

            AppCore.Logger.Log("加载热更代码:" + subGame.dllName + ".dll");
            return Assembly.Load(dllBytes);
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
        public static string GetFileOutputPath(string fileName)
        {
            var path = $"{GetOutputBaseDir()}/{GetPlatformName()}";
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
        public static string GetOutputPathDir()
        {
            var path = $"{GetOutputBaseDir()}/{GetPlatformName()}";
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
        public static string GetOutputBaseDir()
        {
            var path = $"{Application.dataPath.Substring(0, Application.dataPath.Length - 7)}/output";
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
        public static string GetCachePath(string fileName)
        {
            return $"{Application.persistentDataPath}/{fileName}";
        }

        /// <summary>
        /// 获取资源服务器资源文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetFileUrl(string fileName)
        {
            if (_start.ossOptions.bucket.IsNullOrEmpty())
            {
                return String.Empty;
            }

            switch (_start.ossOptions.type)
            {
                case OSSType.Streaming:
                    return Path.Combine(Application.streamingAssetsPath, fileName.ToLower());
                case OSSType.Aliyun:
                    if (_start.ossOptions.enableAccelerate)
                    {
                        return $"https://{_start.ossOptions.bucket}.oss-accelerate.aliyuncs.com/{AppCore.GetPlatformName()}/{fileName.ToLower()}";
                    }

                    return
                        $"https://{_start.ossOptions.bucket}.oss-{_start.ossOptions.region}.aliyuncs.com/{AppCore.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.Tencent:
                    if (_start.ossOptions.enableAccelerate)
                    {
                        return
                            $"https://{_start.ossOptions.bucket}.cos.accelerate.myqcloud.com/{AppCore.GetPlatformName()}/{fileName.ToLower()}";
                    }

                    return
                        $"https://{_start.ossOptions.bucket}.cos.{_start.ossOptions.region}.myqcloud.com/{AppCore.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.URL:
                    return
                        $"{_start.ossOptions.region}{AppCore.GetPlatformName()}/{fileName.Replace(" ", "_").ToLower()}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 获取服务器接口
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetApiUrl(string path)
        {
            if (port == 0)
            {
                return $"{hosting}{path}";
            }

            return $"{hosting}:{port}{path}";
        }


        /// <summary>
        /// 启动一个携程
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return _start.StartCoroutine(enumerator);
        }

        /// <summary>
        /// 停止一个携程
        /// </summary>
        /// <param name="coroutine"></param>
        public static void StopCoroutine(Coroutine coroutine)
        {
            _start.StopCoroutine(coroutine);
        }
    }
}
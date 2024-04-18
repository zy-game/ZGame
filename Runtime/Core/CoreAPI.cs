using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using UnityEngine.Profiling;
using ZGame.Config;
using ZGame.Data;
using ZGame.VFS;
using ZGame.Game;
using ZGame.Language;
using ZGame.Logger;
using ZGame.Networking;
using ZGame.Notify;
using ZGame.Sound;
using ZGame.UI;

namespace ZGame
{

    /// <summary>
    /// 框架入口
    /// </summary>
    public static class CoreAPI
    {
        public class GameFrameworkContent : MonoBehaviour
        {
            private static Dictionary<string, bool> foloutList = new();

            private void OnApplicationQuit()
            {
                CoreAPI.Uninitialized();
            }

            private void FixedUpdate()
            {
                Profiler.BeginSample("GAME FRAMEWORK FIXED UPDATE");
                for (int i = 0; i < _modules.Count; i++)
                {
                    _modules[i].FixedUpdate();
                }

                for (int i = 0; i < _worlds.Count; i++)
                {
                    _worlds[i].FixedUpdate();
                }

                Profiler.EndSample();
            }

            private void Update()
            {
                Profiler.BeginSample("GAME FRAMEWORK UPDATE");
                for (int i = 0; i < _modules.Count; i++)
                {
                    _modules[i].Update();
                }

                for (int i = 0; i < _worlds.Count; i++)
                {
                    _worlds[i].Update();
                }

                Profiler.EndSample();
            }

            private void LateUpdate()
            {
                Profiler.BeginSample("GAME FRAMEWORK LATE UPDATE");
                for (int i = 0; i < _modules.Count; i++)
                {
                    _modules[i].LateUpdate();
                }

                for (int i = 0; i < _worlds.Count; i++)
                {
                    _worlds[i].LateUpdate();
                }

                Profiler.EndSample();
            }

            private void OnDrawGizmos()
            {
                OnDrawModuleGizmos();
            }

            private void OnDrawGizmosSelected()
            {
                OnDrawModuleGizmos();
            }

            void OnDrawModuleGizmos()
            {
                Profiler.BeginSample("GAME FRAMEWORK DRAW GIZOM");
                for (int i = 0; i < _modules.Count; i++)
                {
                    _modules[i].OnDarwGizom();
                }

                for (int i = 0; i < _worlds.Count; i++)
                {
                    _worlds[i].OnDarwGizom();
                }

                Profiler.EndSample();
            }

            private void OnBecameVisible()
            {
                throw new NotImplementedException();
            }

            public static void OnProfile()
            {
#if UNITY_EDITOR

                foreach (var VARIABLE in CoreAPI._modules)
                {
                    string title = VARIABLE.GetType().Name;
                    if (foloutList.ContainsKey(title) is false)
                    {
                        foloutList.Add(title, false);
                    }

                    foloutList[title] = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(foloutList[title], title);
                    if (foloutList[title])
                    {
                        GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                        VARIABLE.OnGUI();
                        GUILayout.EndVertical();
                    }


                    UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();
                }

                for (int i = 0; i < _worlds.Count; i++)
                {
                    _worlds[i].OnGUI();
                }
#endif
            }
        }

        private static GameFrameworkContent _handle;
        private static List<World> _worlds = new List<World>();
        private static List<GameFrameworkModule> _modules = new List<GameFrameworkModule>();

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
        public static LocationDatableManager Location { get; private set; }

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
        public static NetManager Network { get; private set; }

        /// <summary>
        /// 游戏世界
        /// </summary>
        public static World world { get; private set; }


        /// <summary>
        /// 初始化框架
        /// </summary>
        public static async void Initialized()
        {
            Logger = GetOrCreateModule<LoggerManager>();
            Language = GetOrCreateModule<LanguageManager>();
            Datable = GetOrCreateModule<RuntimeDatableManager>();
            VFS = GetOrCreateModule<VFSManager>();
            Location = GetOrCreateModule<LocationDatableManager>();
            UI = GetOrCreateModule<UIManager>();
            Audio = GetOrCreateModule<AudioPlayerManager>();
            Recorder = GetOrCreateModule<RecorderManager>();
            Notify = GetOrCreateModule<NotifyManager>();
            Config = GetOrCreateModule<ConfigManager>();
            Network = GetOrCreateModule<NetManager>();
            _worlds = new List<World>();
            world = NewWorld("GAME_DEFAULT_WORLD");
            _handle = new GameObject("GAME FRAMEWORK ENTRY").AddComponent<GameFrameworkContent>();
            _handle.gameObject.AddComponent<CinemachineBrain>().m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
            _handle.gameObject.SetParent(null, Vector3.zero, Vector3.zero, Vector3.one);
            GameObject.DontDestroyOnLoad(_handle.gameObject);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            UILoading.SetTitle(Language.Query(CommonLanguage.GetConfig));
            UILoading.SetProgress(0);

            if (await VFS.LoadingResourcePackageAsync(ResConfig.instance.defaultPackageName) is not Status.Success)
            {
                UIMsgBox.Show(Language.Query(CommonLanguage.ResLoadFail), GameFrameworkStartup.Quit);
                return;
            }

            if (await VFS.LoadingSubGameEntryPoint(Path.GetFileNameWithoutExtension(GameConfig.instance.path), GameConfig.instance.mode) is not Status.Success)
            {
                return;
            }

            UILoading.Hide();
        }


        /// <summary>
        /// 反初始化框架
        /// </summary>
        public static void Uninitialized()
        {
            Notify.Notify(new AppQuitEventDatable());
            for (int i = 0; i < _modules.Count; i++)
            {
                RefPooled.Release(_modules[i]);
            }

            _modules.Clear();
            Recorder = null;
            VFS = null;
            Datable = null;
            Location = null;
            Language = null;
            UI = null;
            Audio = null;
            Notify = null;
            Config = null;
            Logger = null;
            Network = null;
            // ECS = null;
            world = null;
        }

        /// <summary>
        /// 获取或加载模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrCreateModule<T>(params object[] args) where T : GameFrameworkModule
        {
            return (T)GetOrCreateModule(typeof(T));
        }

        /// <summary>
        /// 获取或加载模块
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GameFrameworkModule GetOrCreateModule(Type type, params object[] args)
        {
            GameFrameworkModule frameworkModule = _modules.Find(m => m.GetType() == type);
            if (frameworkModule is null)
            {
                frameworkModule = (GameFrameworkModule)RefPooled.Spawner(type);
                frameworkModule.OnAwake(args);
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

            RefPooled.Release(frameworkModule);
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
            return GameConfig.instance.URL.GetUrl(path);
        }

        /// <summary>
        /// 启动一个携程
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return _handle.StartCoroutine(enumerator);
        }

        /// <summary>
        /// 停止一个携程
        /// </summary>
        /// <param name="coroutine"></param>
        public static void StopCoroutine(Coroutine coroutine)
        {
            _handle.StopCoroutine(coroutine);
        }

        /// <summary>
        /// 尝试获取指定名字的世界
        /// </summary>
        /// <param name="name"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static bool TryGetWorld(string name, out World world)
        {
            world = GetWorld(name);
            return world != null;
        }

        /// <summary>
        /// 创建世界
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static World NewWorld(string name)
        {
            World world = GetWorld(name);
            if (world != null)
            {
                return world;
            }

            world = World.Create(name);
            _worlds.Add(world);
            return world;
        }

        /// <summary>
        /// 获取世界
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static World GetWorld(string name)
        {
            return _worlds.Find(w => w.name == name);
        }
    }
}
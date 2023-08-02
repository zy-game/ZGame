using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Internal;
using ZEngine;
using ZEngine.VFS;
using ZEngine.Network;
using ZEngine.Options;
using ZEngine.Resource;
using ZEngine.Sound;
using ZEngine.Window;
using ZEngine.World;
using Object = UnityEngine.Object;

public sealed class Engine
{
    public sealed class Cache
    {
        public static CacheTokenHandle Enqueue<T>(T value)
            => RuntimeCacheing.instance.Enqueue(value);

        public static T Dequeue<T>(CacheTokenHandle tokenHandle)
            => RuntimeCacheing.instance.Dequeue<T>(tokenHandle);
    }

    /// <summary>
    /// 控制台
    /// </summary>
    public sealed class Console
    {
        /// <summary>
        /// 在控制台输出一条日志
        /// </summary>
        /// <param name="message"></param>
        public static void Log(object message)
            => Debug.Log($"[INFO]{message}");

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="message"></param>
        public static void Warning(object message)
            => Debug.LogWarning($"[WARNING]{message}");

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="message"></param>
        public static void Error(object message)
            => Debug.LogError($"[ERROR]{message}");
    }

    /// <summary>
    /// 引用池
    /// </summary>
    public sealed class Class
    {
        /// <summary>
        /// 获取一个引用对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Loader<T>() where T : IReference
            => ClassManager.instance.Dequeue<T>();

        /// <summary>
        /// 获取一个引用对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReference Loader(Type type)
            => ClassManager.instance.Dequeue(type);

        /// <summary>
        /// 回收引用对象，但是不释放对象
        /// </summary>
        /// <param name="reference"></param>
        public static void Release(IReference reference)
            => ClassManager.instance.Enqueue(reference);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    public sealed class Json
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string ToJson(object target)
            => JsonConvert.SerializeObject(target);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Parse<T>(string value)
            => JsonConvert.DeserializeObject<T>(value);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object Parse(Type type, string value)
            => JsonConvert.DeserializeObject(value, type);
    }

    /// <summary>
    /// 文件系统
    /// </summary>
    public sealed class FileSystem
    {
        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool Exist(string fileName)
            => VFSManager.instance.Exist(fileName);

        /// <summary>
        /// 获取文件版本
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static VersionOptions GetFileVersion(string fileName)
            => VFSManager.instance.GetFileVersion(fileName);

        /// <summary>
        /// 获取本地缓存文件地址
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetLocalFilePath(string fileName)
            => VFSManager.GetLocalFilePath(fileName);

        /// <summary>
        /// 获取热更文件地址
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetHotfixFilePath(string fileName)
            => fileName;

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IWriteFileExecute WriteFile(string fileName, byte[] bytes, VersionOptions version)
            => VFSManager.instance.WriteFile(fileName, bytes, version);

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IWriteFileExecuteHandle WriteFileAsync(string fileName, byte[] bytes, VersionOptions version)
            => VFSManager.instance.WriteFileAsync(fileName, bytes, version);

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IReadFileExecute ReadFile(string fileName)
            => VFSManager.instance.ReadFile(fileName);

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IReadFileExecuteHandle ReadFileAsync(string fileName)
            => VFSManager.instance.ReadFileAsync(fileName);
    }

    /// <summary>
    /// 游戏管理器
    /// </summary>
    public sealed class Game
    {
        /// <summary>
        /// 当前启用的游戏
        /// </summary>
        public static IGameWorld current => WorldManager.instance.current;

        /// <summary>
        /// 加载游戏
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IGameWorld LoadGameWorld(IGameWorldOptions options) => WorldManager.instance.LoadGameWorld(options);

        /// <summary>
        /// 获取指定名称且已加载的游戏
        /// </summary>
        /// <param name="worldName"></param>
        /// <returns></returns>
        public static IGameWorld GetGameWorld(string worldName) => WorldManager.instance.GetGameWorld(worldName);

        /// <summary>
        /// 关闭已开启的游戏
        /// </summary>
        /// <param name="worldName"></param>
        public static void CloseWorld(string worldName) => WorldManager.instance.CloseWorld(worldName);

        /// <summary>
        /// 启动一个逻辑系统
        /// </summary>
        /// <param name="paramsList">运行逻辑系统所需要的参数</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogicSystemExecuteHandle LaunchLogicSystem<T>(params object[] paramsList) where T : ILogicSystemExecuteHandle
            => WorldManager.instance.LaunchLogicSystem<T>(paramsList);
    }

    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class Resource
    {
        /// <summary>
        /// 获取资源模块版本
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <returns></returns>
        public static ModuleManifest GetModuleManifest(string moduleName)
            => ResourceManager.instance.GetModuleManifest(moduleName);

        /// <summary>
        /// 获取资源包版本
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <param name="bundleName">资源包名</param>
        /// <returns></returns>
        public static BundleManifest GetResourceBundleManifest(string assetPath)
            => ResourceManager.instance.GetResourceBundleManifest(assetPath);

        /// <summary>
        /// 获取资源包版本
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <param name="bundleName">资源包名</param>
        /// <returns></returns>
        public static BundleManifest GetResourceBundleManifest(string moduleName, string bundleName)
            => ResourceManager.instance.GetResourceBundleManifest(moduleName, bundleName);

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public static IAssetRequestExecute<T> LoadAsset<T>(string assetPath) where T : Object
            => ResourceManager.instance.LoadAsset<T>(assetPath);

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="module">资源所在模块</param>
        /// <param name="bundle">资源所在包</param>
        /// <param name="assetName">资源名</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAssetRequestExecute<T> LoadAsset<T>(string module, string bundle, string assetName) where T : Object
            => ResourceManager.instance.LoadAsset<T>($"{module}/{bundle}/{assetName}");

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public static IAssetRequestExecuteHandle<T> LoadAssetAsync<T>(string assetPath) where T : Object
            => ResourceManager.instance.LoadAssetAsync<T>(assetPath);

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="module">资源所在模块</param>
        /// <param name="bundle">资源所在包</param>
        /// <param name="assetName">资源名</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAssetRequestExecuteHandle<T> LoadAssetAsync<T>(string module, string bundle, string assetName) where T : Object
            => ResourceManager.instance.LoadAssetAsync<T>($"{module}/{bundle}/{assetName}");

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="handle">资源句柄</param>
        public static void Release(Object handle)
            => ResourceManager.instance.Release(handle);

        /// <summary>
        /// 预加载资源模块
        /// </summary>
        /// <param name="options">预加载资源选项</param>
        public static IResourcePreloadExecuteHandle PreLoadResourceModule(ResourcePreloadOptions options)
            => ResourceManager.instance.PreLoadResourceModule(options);

        /// <summary>
        /// 检查资源是否需要更新
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ICheckUpdateExecuteHandle CheckUpdateResource(ResourceUpdateOptions options)
            => ResourceManager.instance.CheckUpdateResource(options);
    }

    /// <summary>
    /// 音效
    /// </summary>
    public sealed class Sound
    {
        /// <summary>
        /// 设置默认播放器选项
        /// </summary>
        /// <param name="options"></param>
        public static void SetPlayOptions(ISoundPlayOptions options)
            => SoundManager.instance.SetPlayOptions(options);

        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="soundName"></param>
        /// <returns></returns>
        public static ISoundPlayHandle PlaySound(string soundName, [DefaultValue("default")] string optionsName)
            => SoundManager.instance.PlaySound(soundName, optionsName);

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="soundName"></param>
        /// <returns></returns>
        public static ISoundPlayHandle PlayEffectSound(string soundName, [DefaultValue("default")] string optionsName)
            => SoundManager.instance.PlayEffectSound(soundName, optionsName);

        /// <summary>
        /// 获取播放器设置
        /// </summary>
        /// <param name="optionsName"></param>
        /// <returns></returns>
        public static ISoundPlayOptions GetSoundPlayOptions(string optionsName)
            => SoundManager.instance.GetSoundPlayOptions(optionsName);
    }

    /// <summary>
    /// 用户界面
    /// </summary>
    public sealed class Window
    {
        /// <summary>
        /// 打开指定类型的窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T OpenWindow<T>() where T : IWindowHandle
            => (T)OpenWindow(typeof(T));

        /// <summary>
        /// 打开指定类型的窗口
        /// </summary>
        /// <param name="windowType"></param>
        /// <returns></returns>
        public static IWindowHandle OpenWindow(Type windowType)
            => WindowManager.instance.OpenWindow(windowType);

        /// <summary>
        /// 打开指定类型的窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IOpenedWindowExecuteAsyncHandleHandle<T> OpenWindowAsync<T>() where T : IWindowHandle
            => WindowManager.instance.OpenWindowAsync<T>();

        /// <summary>
        /// 打开指定类型的窗口
        /// </summary>
        /// <param name="windowType"></param>
        /// <returns></returns>
        public static IOpenedWindowExecuteAsyncHandleHandle<IWindowHandle> OpenWindowAsync(Type windowType)
            => WindowManager.instance.OpenWindowAsync(windowType);

        /// <summary>
        /// 获取指定类型的窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetWindow<T>() where T : IWindowHandle
            => (T)GetWindow(typeof(T));

        /// <summary>
        /// 获取指定类型的窗口
        /// </summary>
        /// <param name="windowType"></param>
        /// <returns></returns>
        public static IWindowHandle GetWindow(Type windowType)
            => WindowManager.instance.GetWindow(windowType);

        /// <summary>
        /// 关闭指定窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Close<T>() where T : IWindowHandle
            => Close(typeof(T));

        /// <summary>
        /// 关闭指定的窗口
        /// </summary>
        /// <param name="windowType"></param>
        public static void Close(Type windowType)
            => WindowManager.instance.Close(windowType);
    }

    /// <summary>
    /// 网络管理
    /// </summary>
    public sealed class Network
    {
        /// <summary>
        /// 请求数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static INetworkRequestExecuteAsyncHandleHandle<string> Get(string url, object data, Dictionary<string, object> header = default)
            => NetworkManager.instance.Get<string>(url, data, header);

        /// <summary>
        /// 请求数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static INetworkRequestExecuteAsyncHandleHandle<T> GetAsync<T>(string url, object data, Dictionary<string, object> header = default)
            => NetworkManager.instance.Get<T>(url, data, header);

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static INetworkRequestExecuteAsyncHandleHandle<string> Post(string url, object data, Dictionary<string, object> header = default)
            => NetworkManager.instance.Post(url, data, header);

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static INetworkRequestExecuteAsyncHandleHandle<T> Post<T>(string url, object data, Dictionary<string, object> header = default)
            => NetworkManager.instance.Post<T>(url, data, header);
    }
}
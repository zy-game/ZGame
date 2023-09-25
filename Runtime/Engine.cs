using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.Video;
using ZEngine;
using ZEngine.Cache;
using ZEngine.VFS;
using ZEngine.Network;
using ZEngine.Resource;
using ZEngine.Window;
using ZEngine.Game;
using ZEngine.Playable;
using ZEngine.Utility;
using ZEngine.ZJson;
using Object = UnityEngine.Object;

public sealed class Engine
{
    /// <summary>
    /// 退出播放
    /// </summary>
    public static void Quit()
    {
        Extension.StopAll();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    /// <summary>
    /// 获取当前运行时平台名(小写)
    /// </summary>
    /// <returns></returns>
    public static string GetPlatfrom()
    {
#if UNITY_ANDROID
            return "android";
#elif UNITY_IPHONE
            return "ios";
#else
        return "windows";
#endif
    }

    /// <summary>
    /// 获取随机名
    /// </summary>
    /// <returns></returns>
    public static string RandomName()
    {
        return Guid.NewGuid().ToString().Replace("-", "");
    }

    /// <summary>
    /// 获取热更资源路径
    /// </summary>
    /// <param name="url"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetHotfixPath(string url, string name)
    {
        return $"{url}/{Engine.GetPlatfrom()}/{name}";
    }

    /// <summary>
    /// 获取本地缓存文件路径
    /// </summary>
    /// <param name="fileName">文件名，不包含扩展名</param>
    /// <returns></returns>
    public static string GetLocalFilePath(string fileName)
    {
        return $"{Application.persistentDataPath}/{fileName}";
    }

    /// <summary>
    /// 缓存区
    /// </summary>
    public sealed class Cache
    {
        /// <summary>
        /// 缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Handle(string key, object value)
            => ObjectPoolManager.instance.Handle(key, value);

        /// <summary>
        /// 尝试获取缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryGetValue<T>(string key, out T value)
            => ObjectPoolManager.instance.TryGetValue(key, out value);

        /// <summary>
        /// 设置缓存管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetCacheHandle<T>() where T : IObjectPoolHandle
            => ObjectPoolManager.instance.SetCacheHandle(typeof(T));

        /// <summary>
        /// 设置缓存管道
        /// </summary>
        /// <param name="handleType"></param>
        public static void SetCacheHandle(Type handleType)
            => ObjectPoolManager.instance.SetCacheHandle(handleType);

        /// <summary>
        /// 移除缓存管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RemoveCacheHandle<T>() where T : IObjectPoolHandle
            => ObjectPoolManager.instance.RemoveCacheHandle(typeof(T));

        /// <summary>
        /// 移除缓存管道
        /// </summary>
        /// <param name="handleType"></param>
        public static void RemoveCacheHandle(Type handleType)
            => ObjectPoolManager.instance.RemoveCacheHandle(handleType);

        /// <summary>
        /// 移除自定的类型的缓存区域
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RemoveCacheArea<T>()
            => RemoveCacheArea(typeof(T));

        /// <summary>
        /// 移除自定的类型的缓存区域
        /// </summary>
        /// <param name="type"></param>
        public static void RemoveCacheArea(Type type)
            => ObjectPoolManager.instance.RemoveCacheArea(type);
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
            => Debug.Log($"[INFO] {message}");

        /// <summary>
        /// 在控制台输出一条日志
        /// </summary>
        /// <param name="message"></param>
        public static void Log(params object[] message)
            => Debug.Log($"[INFO] {string.Join(" ", message)}");

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="message"></param>
        public static void Warning(object message)
            => Debug.LogWarning($"[WARNING] {message}");

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="message"></param>
        public static void Warning(params object[] message)
            => Debug.LogWarning($"[WARNING] {string.Join("\n", message)}");

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="message"></param>
        public static void Error(object message)
            => Debug.LogError($"[ERROR] {message}");

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="message"></param>
        public static void Error(params object[] message)
            => Debug.LogError($"[ERROR] {string.Join("\n", message)}");
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
        public static IReadFileExecute ReadFile(string fileName, VersionOptions version = null)
            => VFSManager.instance.ReadFile(fileName, version);

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IReadFileExecuteHandle ReadFileAsync(string fileName, VersionOptions version = null)
            => VFSManager.instance.ReadFileAsync(fileName, version);
    }

    /// <summary>
    /// 游戏管理器
    /// </summary>
    public sealed class Game
    {
        /// <summary>
        /// 打开或创建一个World
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IWorld OpenWorld(WorldOptions options)
            => GameManager.instance.OpenWorld(options);

        /// <summary>
        /// 获取指定名称的World
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IWorld GetWorld(string name)
            => GameManager.instance.Find(name);

        /// <summary>
        /// 关闭指定的World
        /// </summary>
        /// <param name="name"></param>
        public static void CloseWorld(string name)
            => GameManager.instance.CloseWorld(name);

        /// <summary>
        /// 创建实体对象
        /// </summary>
        /// <returns></returns>
        public static IEntity CreateEntity()
            => GameManager.instance.CreateEntity();

        /// <summary>
        /// 删除实体对象
        /// </summary>
        /// <param name="id"></param>
        public static void DestroyEntity(int id)
            => GameManager.instance.DestroyEntity(id);

        /// <summary>
        /// 查找实体对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IEntity FindEntity(int id)
            => GameManager.instance.Find(id);

        /// <summary>
        /// 获取实体所有组件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IEntityComponent[] GetComponents(int id)
            => GameManager.instance.GetComponents(id);

        /// <summary>
        /// 获取相同类型的组件列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEntityComponent[] GetComponents(Type type)
            => GameManager.instance.GetComponents(type);

        /// <summary>
        /// 加载游戏逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void LoadGameLogicSystem<T>() where T : IGameLogicSystem
            => LoadGameLogicSystem(typeof(T));

        /// <summary>
        /// 加载游戏逻辑系统
        /// </summary>
        /// <param name="logicType"></param>
        public static void LoadGameLogicSystem(Type logicType)
            => GameManager.instance.LoadGameLogicSystem(logicType);

        /// <summary>
        /// 卸载游戏逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void UnloadGameLogicSystem<T>() where T : IGameLogicSystem
            => UnloadGameLogicSystem(typeof(T));

        /// <summary>
        /// 卸载游戏逻辑系统
        /// </summary>
        /// <param name="logicType"></param>
        public static void UnloadGameLogicSystem(Type logicType)
            => GameManager.instance.UnloadGameLogicSystem(logicType);
    }

    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class Resource
    {
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public static IRequestAssetExecute<T> LoadAsset<T>(string assetPath) where T : Object
            => ResourceManager.instance.LoadAsset<T>(assetPath);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public static IRequestAssetExecuteHandle<T> LoadAssetAsync<T>(string assetPath) where T : Object
            => ResourceManager.instance.LoadAssetAsync<T>(assetPath);

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="handle">资源句柄</param>
        public static void Dispose(Object handle)
            => ResourceManager.instance.Dispose(handle);

        /// <summary>
        /// 预加载资源模块
        /// </summary>
        public static IResourceModuleLoaderExecuteHandle LoaderResourceModule(params ModuleOptions[] options)
            => ResourceManager.instance.LoaderResourceModule(options);

        /// <summary>
        /// 检查资源是否需要更新
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ICheckResourceUpdateExecuteHandle CheckModuleResourceUpdate(params ModuleOptions[] options)
            => ResourceManager.instance.CheckModuleResourceUpdate(options);
    }

    /// <summary>
    /// 播放器
    /// </summary>
    public sealed class Playable
    {
        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="volume"></param>
        /// <param name="isFullScene"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public static IPlayableHandle PlayVideo(string url, float volume = 1, RenderTexture scene = null, bool isLoop = false, Switch isCache = Switch.Off)
            => PlayableManager.instance.PlayVideo(url, volume, scene, isLoop, isCache);

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="soundName"></param>
        /// <param name="volume"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public static IPlayableHandle PlaySound(string url, float volume = 1, bool isLoop = false, Switch isCache = Switch.Off)
            => PlayableManager.instance.PlaySound(url, volume, isLoop, isCache);

        /// <summary>
        /// 获取播放器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IPlayableHandle GetPlayableHandle(string name)
            => PlayableManager.instance.GetPlayableHandle(name);
    }

    /// <summary>
    /// 用户界面
    /// </summary>
    public sealed class Window
    {
        /// <summary>
        /// 小提示窗口
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public static Toast Toast(string test)
            => OpenWindow<Toast>().SetToast(test);

        /// <summary>
        /// 提示消息窗口
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public static MsgBox MsgBox(string text)
            => MsgBox("Tips", text);

        /// <summary>
        /// 提示消息窗口
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public static MsgBox MsgBox(string tips, string text, Action ok = null, Action cancel = null, string okText = "OK", string cancelText = "Cancel")
            => OpenWindow<MsgBox>().SetBox(tips, text, ok, cancel, okText, cancelText);

        /// <summary>
        /// 等待窗口
        /// </summary>
        /// <param name="text"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Waiting Wait(string text, float time = 0, ISubscriber subscribe = null)
            => OpenWindow<Waiting>().SetWait(text, time, subscribe);

        /// <summary>
        /// 打开指定类型的窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T OpenWindow<T>() where T : UIWindow
            => (T)OpenWindow(typeof(T));

        /// <summary>
        /// 打开指定类型的窗口
        /// </summary>
        /// <param name="windowType"></param>
        /// <returns></returns>
        public static UIWindow OpenWindow(Type windowType)
            => WindowManager.instance.OpenWindow(windowType);

        /// <summary>
        /// 获取指定类型的窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetWindow<T>() where T : UIWindow
            => (T)GetWindow(typeof(T));

        /// <summary>
        /// 获取指定类型的窗口
        /// </summary>
        /// <param name="windowType"></param>
        /// <returns></returns>
        public static UIWindow GetWindow(Type windowType)
            => WindowManager.instance.GetWindow(windowType);

        /// <summary>
        /// 关闭指定窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Close<T>(bool isCache = false) where T : UIWindow
            => Close(typeof(T), isCache);

        /// <summary>
        /// 关闭指定的窗口
        /// </summary>
        /// <param name="windowType"></param>
        public static void Close(Type windowType, bool isCache = false)
            => WindowManager.instance.Close(windowType, isCache);

        /// <summary>
        /// 显示界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Show<T>() where T : UIWindow
            => Show(typeof(T));

        /// <summary>
        /// 显示界面
        /// </summary>
        /// <param name="type"></param>
        public static void Show(Type type)
            => WindowManager.instance.Show(type);

        /// <summary>
        /// 隐藏界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Hide<T>() where T : UIWindow
            => Hide(typeof(T));

        /// <summary>
        /// 隐藏界面
        /// </summary>
        /// <param name="type"></param>
        public static void Hide(Type type)
            => WindowManager.instance.Hide(type);
    }

    /// <summary>
    /// 网络管理
    /// </summary>
    public sealed class Network
    {
        /// <summary>
        /// 订阅消息处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SubscribeMessageHandle<T>(ISubscriber<T> subscribe) where T : IMessaged
            => SubscribeMessageHandle(typeof(T), subscribe);

        /// <summary>
        /// 订阅消息处理
        /// </summary>
        /// <param name="type"></param>
        public static void SubscribeMessageHandle(Type type, ISubscriber subscribe)
            => MessageDispatcher.instance.Subscribe(type, subscribe);

        /// <summary>
        /// 取消消息订阅管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void UnsubscribeMessageHandle<T>(ISubscriber<T> subscribe) where T : IMessaged
            => UnsubscribeMessageHandle(typeof(T), subscribe);

        /// <summary>
        /// 取消消息订阅管道
        /// </summary>
        /// <param name="type"></param>
        public static void UnsubscribeMessageHandle(Type type, ISubscriber subscribe)
            => MessageDispatcher.instance.Unsubscribe(type, subscribe);


        /// <summary>
        /// 请求数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IWebRequestExecuteHandle<T> Get<T>(string url, object data = default, Dictionary<string, object> header = default)
            => NetworkManager.instance.Request<T>(url, data, NetworkRequestMethod.GET, header);

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IWebRequestExecuteHandle<T> Post<T>(string url, object data, Dictionary<string, object> header = default)
            => NetworkManager.instance.Request<T>(url, data, NetworkRequestMethod.POST, header);

        /// <summary>
        /// 多文件下载
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IDownloadExecuteHandle Download(params DownloadOptions[] urlList)
            => NetworkManager.instance.Download(urlList);

        /// <summary>
        /// 连接远程地址
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public static IExecuteHandle<IChannel> Connect(string address, int id = 0)
            => NetworkManager.instance.Connect(address, id);

        /// <summary>
        /// 写入网络消息，如果网络未连接则自动尝试链接，并在链接成功后写入消息
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <returns></returns>
        public static IExecuteHandle WriteAndFlush(string address, IMessaged messagePackage)
            => NetworkManager.instance.WriteAndFlush(address, messagePackage);

        /// <summary>
        /// 写入网络消息,并等待响应
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <typeparam name="T">等待响应的消息类型</typeparam>
        /// <returns></returns>
        public static ISubscriber<T> WriteAndFlush<T>(string address, IMessaged messagePackage) where T : IMessaged
            => NetworkManager.instance.WriteAndFlush<T>(address, messagePackage);

        /// <summary>
        /// 关闭网络连接
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public static IExecuteHandle Close(string address)
            => NetworkManager.instance.Close(address);
    }
}
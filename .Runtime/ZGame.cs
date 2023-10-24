using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
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
using ZEngine.Language;
using ZEngine.Utility;
using ZEngine.ZJson;
using Object = UnityEngine.Object;

public sealed class ZGame
{
    public static void Initialize()
        => Custom.Initialize();

    /// <summary>
    /// 退出播放
    /// </summary>
    public static void Quit()
        => Custom.Quit();

    /// <summary>
    /// 获取当前运行时平台名(小写)
    /// </summary>
    /// <returns></returns>
    public static string GetPlatfrom()
        => Custom.GetPlatfrom();

    /// <summary>
    /// 获取随机名
    /// </summary>
    /// <returns></returns>
    public static string RandomName()
        => Custom.RandomName();

    /// <summary>
    /// 获取随机数
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static float Random(float l, float r)
        => Custom.Random(l, r);

    /// <summary>
    /// 获取随机数
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static int Random(int l, int r)
        => Custom.Random(l, r);

    /// <summary>
    /// 获取热更资源路径
    /// </summary>
    /// <param name="url"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetHotfixPath(string url, string name)
        => Custom.GetHotfixPath(url, name);

    /// <summary>
    /// 获取本地缓存文件路径
    /// </summary>
    /// <param name="fileName">文件名，不包含扩展名</param>
    /// <returns></returns>
    public static string GetLocalFilePath(string fileName)
        => Custom.GetLocalFilePath(fileName);

    public sealed class Datable
    {
        /// <summary>
        /// 添加运行时数据句柄
        /// </summary>
        /// <param name="runtimeDatableHandle"></param>
        /// <typeparam name="T"></typeparam>
        public static void Add(object runtimeDatableHandle)
            => DatableManager.instance.Add(runtimeDatableHandle, runtimeDatableHandle);

        /// <summary>
        /// 添加运行时数据句柄
        /// </summary>
        /// <param name="runtimeDatableHandle"></param>
        /// <typeparam name="T"></typeparam>
        public static void Add(object key, object runtimeDatableHandle)
            => DatableManager.instance.Add(key, runtimeDatableHandle);

        /// <summary>
        /// 尝试获取数据句柄
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryGetValue<T>(object key, out T result)
            => DatableManager.instance.TryGetValue<T>(key, out result);

        /// <summary>
        /// 获取运行时数据句柄
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDatable<T>(object key)
            => DatableManager.instance.GetDatable<T>(key);

        /// <summary>
        /// 获取运行时数据句柄
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDatable<T>(Func<T, bool> func)
            => DatableManager.instance.GetDatable<T>(func);

        /// <summary>
        /// 是否存在数据句柄
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Equals<T>(object key)
            => GetDatable<T>(key) != null;

        /// <summary>
        /// 回收数据句柄
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        public static void Release(object key, bool isCache = false)
            => DatableManager.instance.Release(key, isCache);

        /// <summary>
        /// 清理所有相同类型的数据
        /// </summary>
        /// <param name="isCache"></param>
        /// <typeparam name="T"></typeparam>
        public static void Clear<T>(bool isCache = false)
            => Clear(typeof(T), isCache);

        /// <summary>
        /// 清理所有相同类型的数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isCache"></param>
        public static void Clear(Type type, bool isCache = false)
            => DatableManager.instance.Clear(type, isCache);
    }

    /// <summary>
    /// 本地化
    /// </summary>
    public sealed class Localization
    {
        /// <summary>
        /// 获取本地化配置
        /// </summary>
        /// <param name="id">配置ID</param>
        /// <returns></returns>
        public static ILanguageOptions GetLocalizationOptions(int id)
            => LanguageManager.instance.Switch(id);

        /// <summary>
        /// 切换语言类型
        /// </summary>
        /// <param name="define"></param>
        public static UniTask SwitchLanguage(LanguageDefine define)
            => LanguageManager.instance.SwitchLanguage(define);
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
        public static void Enqueue(object key, object value)
            => ObjectManager.instance.Enqueue(key, value);

        /// <summary>
        /// 尝试获取缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryGetValue<T>(string key, out T value)
            => ObjectManager.instance.TryGetValue(key, out value);

        /// <summary>
        /// 设置缓存管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetCacheHandle<T>() where T : IObjectPoolPipeline
            => ObjectManager.instance.SetCacheHandle(typeof(T));

        /// <summary>
        /// 设置缓存管道
        /// </summary>
        /// <param name="handleType"></param>
        public static void SetCacheHandle(Type handleType)
            => ObjectManager.instance.SetCacheHandle(handleType);

        /// <summary>
        /// 移除缓存管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RemoveCacheHandle<T>() where T : IObjectPoolPipeline
            => ObjectManager.instance.RemoveCacheHandle(typeof(T));

        /// <summary>
        /// 移除缓存管道
        /// </summary>
        /// <param name="handleType"></param>
        public static void RemoveCacheHandle(Type handleType)
            => ObjectManager.instance.RemoveCacheHandle(handleType);

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
            => ObjectManager.instance.RemoveCacheArea(type);
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
            => ZEngine.Console.instance.Log($"[INFO] {message}");

        /// <summary>
        /// 在控制台输出一条日志
        /// </summary>
        /// <param name="message"></param>
        public static void Log(params object[] message)
            => ZEngine.Console.instance.Log($"[INFO] {string.Join(" ", message)}");

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="message"></param>
        public static void Warning(object message)
            => ZEngine.Console.instance.Warning($"[WARNING] {message}");

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="message"></param>
        public static void Warning(params object[] message)
            => ZEngine.Console.instance.Warning($"[WARNING] {string.Join(" ", message)}");

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="message"></param>
        public static void Error(object message)
            => ZEngine.Console.instance.Error($"[ERROR] {message}");

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="message"></param>
        public static void Error(params object[] message)
            => ZEngine.Console.instance.Error($"[ERROR] {string.Join(" ", message)}");
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
        public static int GetFileVersion(string fileName)
            => VFSManager.instance.GetFileVersion(fileName);

        /// <summary>
        /// 检查文件版本是否一致
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool CheckFileVersion(string fileName, int version)
            => GetFileVersion(fileName) == version;

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IWriteFileResult WriteFile(string fileName, byte[] bytes, int version)
            => VFSManager.instance.WriteFile(fileName, bytes, version);

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static UniTask<IWriteFileResult> WriteFileAsync(string fileName, byte[] bytes, int version)
            => VFSManager.instance.WriteFileAsync(fileName, bytes, version);

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IReadFileResult ReadFile(string fileName, int version)
            => VFSManager.instance.ReadFile(fileName, version);

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static UniTask<IReadFileResult> ReadFileAsync(string fileName, int version)
            => VFSManager.instance.ReadFileAsync(fileName, version);
    }

    /// <summary>
    /// 游戏管理器
    /// </summary>
    public sealed class Game
    {
        /// <summary>
        /// 加载DLL
        /// </summary>
        /// <param name="gameEntryOptions"></param>
        /// <returns></returns>
        public static UniTask<IDllImportReslt> LoadGameLogic(GameEntryOptions gameEntryOptions)
            => DllImport.instance.Import(gameEntryOptions);
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
        public static IRequestResourceObjectResult LoadAsset(string assetPath)
            => ResourceManager.instance.LoadAsset(assetPath);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public static UniTask<IRequestResourceObjectResult> LoadAssetAsync(string assetPath)
            => ResourceManager.instance.LoadAssetAsync(assetPath);

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="handle">资源句柄</param>
        public static void Release(Object handle)
            => ResourceManager.instance.Release(handle);

        /// <summary>
        /// 预加载资源模块
        /// </summary>
        public static UniTask<IRequestResourceModuleLoadResult> LoaderResourceModule(IProgressHandle gameProgressHandle, params ModuleOptions[] options)
            => ResourceManager.instance.LoadingResourceModule(gameProgressHandle, options);

        /// <summary>
        /// 加载单个资源包
        /// </summary>
        /// <param name="progressHandle"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static UniTask<IRequestResourceBundleResult> LoadResourceBundle(IProgressHandle progressHandle, params string[] url)
            => default;

        /// <summary>
        /// 检查资源是否需要更新
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static UniTask<IRequestResourceModuleUpdateResult> CheckModuleResourceUpdate(IProgressHandle gameProgressHandle, params ModuleOptions[] options)
            => ResourceManager.instance.CheckModuleResourceUpdate(gameProgressHandle, options);
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
        public static UniTask<bool> MsgBoxAsync(string text)
            => OpenWindow<MsgBox>().SetBox("提示", text, "OK", "Cancel");

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
        public static Waiting Wait(string text, float time = 0, Action subscribe = null)
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

        /// <summary>
        /// UI事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        public static void OnNotify<T>(string name, params object[] args) where T : UIWindow
            => WindowManager.instance.OnEvent(typeof(T), name, args);

        /// <summary>
        /// UI事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        public static void OnNotify(string name, params object[] args)
            => WindowManager.instance.OnEvent(null, name, args);

        public static void Clear()
            => WindowManager.instance.Clear();
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
        public static void SubscribeMessageRecvieHandle<T>() where T : IMessageRecvierHandle
            => SubscribeMessageRecvieHandle(typeof(T));

        /// <summary>
        /// 订阅消息处理
        /// </summary>
        /// <param name="type"></param>
        public static void SubscribeMessageRecvieHandle(Type type)
            => IMessageRecvierHandle.Create(type);

        /// <summary>
        /// 取消消息订阅管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void UnsubscribeMessageRecvieHandle<T>() where T : IMessageRecvierHandle
            => UnsubscribeMessageRecvieHandle(typeof(T));

        /// <summary>
        /// 取消消息订阅管道
        /// </summary>
        /// <param name="type"></param>
        public static void UnsubscribeMessageRecvieHandle(Type type)
            => IMessageRecvierHandle.Release(type);

        /// <summary>
        /// 请求数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static UniTask<IWebRequestResult<T>> Get<T>(string url, object data = default, Dictionary<string, object> header = default)
            => NetworkManager.instance.Get<T>(url, header);

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static UniTask<IWebRequestResult<T>> Post<T>(string url, object data, Dictionary<string, object> header = default)
            => NetworkManager.instance.Post<T>(url, data, header);

        /// <summary>
        /// 多文件下载
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static UniTask<IDownloadResult> Download(IProgressHandle gameProgressHandle, params DownloadOptions[] urlList)
            => NetworkManager.instance.Download(gameProgressHandle, urlList);

        /// <summary>
        /// 连接远程地址
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public static UniTask<IChannel> Connect<T>(string address, int id = 0) where T : IChannel
            => NetworkManager.instance.Connect<T>(address, id);

        /// <summary>
        /// 写入网络消息，如果网络未连接则自动尝试链接，并在链接成功后写入消息
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <returns></returns>
        public static void WriteAndFlush(string address, IMessaged messagePackage)
            => NetworkManager.instance.WriteAndFlush(address, messagePackage);

        /// <summary>
        /// 关闭网络连接
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public static UniTask<IChannel> Close(string address)
            => NetworkManager.instance.Close(address);
    }
}
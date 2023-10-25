using System;
using UnityEngine;

namespace ZGame.Window
{
    public interface IUIBindComponent : IEntity
    {
    }

    public interface IGameWindow : IEntity
    {
        string name { get; }
        GameObject gameObject { get; }
    }

    public interface IMessageBox : IGameWindow
    {
        IMessageBox Setup(string title, string message, string okString, Action ok, string cancelString = "", Action cancel = null);

        public static IMessageBox Create(string message)
            => Create(message, () => { });

        public static IMessageBox Create(string message, Action ok, Action cancel = null)
            => Create("Tips", message, ok, cancel);

        public static IMessageBox Create(string title, string message, Action ok, Action cancel = null)
            => Create(title, message, "OK", ok, "Cancel", cancel);

        public static IMessageBox Create(string title, string message, string okString, Action ok, string cancelString, Action cancel = null)
            => SystemManager.windowSystem.Open<DefaultMessageBoxer>().Setup(title, message, okString, ok, cancelString, cancel);

        class DefaultMessageBoxer : IMessageBox
        {
            public string guid { get; } = ID.New();
            public string name { get; }
            public GameObject gameObject { get; }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public IMessageBox Setup(string title, string message, string okString, Action ok, string cancelString = "", Action cancel = null)
            {
                throw new NotImplementedException();
            }
        }
    }

    public interface IWaiting : IGameWindow
    {
        public static IWaiting Create(float timeout = 0)
        {
            return default;
        }
    }

    public interface ITips : IGameWindow
    {
        public static ITips Create(string tips)
        {
            return default;
        }
    }

    public interface ILoading : IGameWindow
    {
        void SetProgress(float progress);

        public static ILoading Create()
        {
            return default;
        }
    }

    public static class Extension
    {
        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Open<T>(this IWindowSystem system) where T : IGameWindow
            => (T)system.Open(typeof(T));

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetWindow<T>(this IWindowSystem system) where T : IGameWindow
            => (T)system.GetWindow(typeof(T));

        /// <summary>
        /// 窗口是否显示或是否打开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasShow<T>(this IWindowSystem system) where T : IGameWindow
            => system.HasShow(typeof(T));

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Active<T>(this IWindowSystem system) where T : IGameWindow
            => system.Active(typeof(T));

        /// <summary>
        /// 窗口失活
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Inactive<T>(this IWindowSystem system) where T : IGameWindow
            => system.Inactive(typeof(T));

        /// <summary>
        /// 将窗口置于最前端
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Focus<T>(this IWindowSystem system) where T : IGameWindow
            => system.Focus(typeof(T));
    }

    /// <summary>
    /// 游戏窗口界面管理
    /// </summary>
    public interface IWindowSystem : ISystem
    {
        /// <summary>
        /// 将窗口置于最前端
        /// </summary>
        /// <param name="type"></param>
        void Focus(Type type);

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IGameWindow Open(Type type);

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IGameWindow GetWindow(Type type);

        /// <summary>
        /// 窗口是否显示或是否打开
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool HasShow(Type type);

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <param name="type"></param>
        void Active(Type type);

        /// <summary>
        /// 窗口失活
        /// </summary>
        /// <param name="type"></param>
        void Inactive(Type type);
    }
}
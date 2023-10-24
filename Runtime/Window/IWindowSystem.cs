using System;

namespace ZGame.Window
{
    public interface IGameWindow : IEntity
    {
    }

    public interface IMessageBox : IGameWindow
    {
        public IMessageBox Setup(string message)
            => Setup(message, IEvent.Empty);

        public IMessageBox Setup(string message, IEvent ok, IEvent cancel = null)
            => Setup("Tips", message, ok, cancel);

        public IMessageBox Setup(string title, string message, IEvent ok, IEvent cancel = null)
            => Setup(title, message, "OK", ok, cancel is null ? "" : "Cancel", cancel);

        IMessageBox Setup(string title, string message, string okString, IEvent ok, string cancelString = "", IEvent cancel = null);
    }

    public interface ITips : IGameWindow
    {
    }

    public interface IProgress : IGameWindow
    {
        void SetProgress(float progress);
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
namespace ZGame.Window
{
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
}
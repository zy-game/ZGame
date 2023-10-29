namespace ZGame.Window
{
    public static class Extension
    {
        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Open<T>(this WindowManager system) where T : GameWindow
            => (T)system.Open(typeof(T));

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetWindow<T>(this WindowManager system) where T : GameWindow
            => (T)system.GetWindow(typeof(T));

        /// <summary>
        /// 窗口是否显示或是否打开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasShow<T>(this WindowManager system) where T : GameWindow
            => system.HasShow(typeof(T));

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Active<T>(this WindowManager system) where T : GameWindow
            => system.Active(typeof(T));

        /// <summary>
        /// 窗口失活
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Inactive<T>(this WindowManager system) where T : GameWindow
            => system.Inactive(typeof(T));

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GeOrOpentWindow<T>(this WindowManager system) where T : GameWindow
        {
            T window = GetWindow<T>(system);
            if (window is null)
            {
                return Open<T>(system);
            }

            return window;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="system"></param>
        /// <typeparam name="T"></typeparam>
        public static void Close<T>(this WindowManager system) where T : GameWindow
            => system.Close(typeof(T));
    }
}
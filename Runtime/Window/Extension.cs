namespace ZGame.Window
{
    public static partial class Extension
    {
        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Open<T>(this UIManager system) where T : UIBase
            => (T)system.Open(typeof(T));

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetWindow<T>(this UIManager system) where T : UIBase
            => (T)system.GetWindow(typeof(T));

        /// <summary>
        /// 窗口是否显示或是否打开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasShow<T>(this UIManager system) where T : UIBase
            => system.HasShow(typeof(T));

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Active<T>(this UIManager system) where T : UIBase
            => system.Active(typeof(T));

        /// <summary>
        /// 窗口失活
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Inactive<T>(this UIManager system) where T : UIBase
            => system.Inactive(typeof(T));

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GeOrOpentWindow<T>(this UIManager system) where T : UIBase
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
        public static void Close<T>(this UIManager system) where T : UIBase
            => system.Close(typeof(T));
    }
}
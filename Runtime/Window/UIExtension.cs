using System;

namespace ZEngine.Window
{
    public static class UIExtension
    {
        /// <summary>
        /// 倒计时
        /// </summary>
        /// <param name="window"></param>
        /// <param name="count">总时长</param>
        /// <param name="interval">间隔时长</param>
        /// <param name="format">格式化样式</param>
        /// <param name="callback">刷新回调</param>
        /// <returns></returns>
        public static IUITimingBindPipeline Countdown(this UIWindow window, float count, float interval, Action<IUITimingBindPipeline> callback)
        {
            return IUITimingBindPipeline.Create(window, count, interval, callback);
        }
    }
}
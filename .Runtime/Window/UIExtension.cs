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
        public static IUITimingBindPipeline BindTiming(this UIWindow window, string path, float time, float interval, string text)
        {
            return IUITimingBindPipeline.Create(window, path, time, interval, text);
        }

        public static IUIButtonBindPipeline BindButton(this UIWindow window, string path, Action<IUIButtonBindPipeline> clickCallback)
        {
            return IUIButtonBindPipeline.Create(window, path, clickCallback);
        }

        public static IUISliderBindPipeline BindSlider(this UIWindow window, string path, float value, Action<float> callback = null)
        {
            if (callback is null)
            {
                return IUISliderBindPipeline.Create(window, path);
            }

            return IUISliderBindPipeline.Create(window, path, value, callback);
        }

        public static IUIInputFieldBindPipeline BindInputField(this UIWindow window, string path, string value, Action<string> callback)
        {
            return IUIInputFieldBindPipeline.Create(window, path, value, callback);
        }

        public static IUISpriteBindPipeline BindImage(this UIWindow window, string path)
        {
            return IUISpriteBindPipeline.Create(window, path);
        }

        public static IUITextBindPipeline BindText(this UIWindow window, string path)
        {
            return IUITextBindPipeline.Create(window, path);
        }

        public static IUIToggleBindPipeline BindToggle(this UIWindow window, string path, Action<bool> callback)
        {
            return IUIToggleBindPipeline.Create(window, path, callback);
        }
    }
}
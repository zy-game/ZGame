using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ZGame.UI
{
    /// <summary>
    /// 消息提示框
    /// </summary>
    [RefPath("Resources/Tips")]
    [UIOptions(UILayer.Notification, SceneType.Addition, CacheType.Permanent)]
    public sealed class UITips : UIBase
    {
        public override async void Enable(params object[] args)
        {
            base.Enable(args);
            SetText(args[0].ToString());
            float timeout = (float)args[1];
            Action onFinish = args[2] as Action;
            float time = (float)args[1];
            await UniTask.Delay((int)(time * 1000));
            GameFrameworkEntry.UI.Inactive<UITips>();
            onFinish?.Invoke();
        }

        private void SetText(string content)
        {
            TMP_Text[] texts = this.gameObject.GetComponentsInChildren<TMP_Text>();
            foreach (var VARIABLE in texts)
            {
                if (VARIABLE.name.Equals("content") is false)
                {
                    continue;
                }

                VARIABLE.SetText(content);
            }
        }

        /// <summary>
        /// 显示消息提示框
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="timeout">提示框显示时长</param>
        public static void Show(string content, float timeout = 3)
        {
            Show(content, timeout, null);
        }

        /// <summary>
        /// 显示消息提示框
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="timeout">提示框显示时长</param>
        /// <param name="onFinish">提示框关闭回调</param>
        public static void Show(string content, float timeout, Action onFinish)
        {
            Debug.Log("tips:" + content);
            GameFrameworkEntry.UI.Active<UITips>(new object[] { content, timeout, onFinish });
        }

        /// <summary>
        /// 显示消息提示框
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="timeout">提示框显示时长</param>
        /// <returns></returns>
        public static UniTask ShowAsync(string content, float timeout = 3)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            Show(content, timeout, () => { tcs.TrySetResult(); });
            return tcs.Task;
        }
    }
}
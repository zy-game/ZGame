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
    public sealed class UITips : UIBase
    {
        private string title;
        private Action finish;
        private float time;

        public override void Start(params object[] args)
        {
            this.title = args[0].ToString();
            this.time = (float)args[1];
            this.finish = (Action)args[2];
        }

        public override async void Enable()
        {
            base.Enable();
            TMP_Text[] texts = this.gameObject.GetComponentsInChildren<TMP_Text>();
            foreach (var VARIABLE in texts)
            {
                if (VARIABLE.name.Equals("content") is false)
                {
                    continue;
                }

                VARIABLE.SetText(title);
            }

            await UniTask.Delay((int)(time * 1000));
            AppCore.UI.Close<UITips>();
            finish?.Invoke();
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
            AppCore.Logger.Log("tips:" + content);
            AppCore.UI.Show<UITips>(UILayer.Notification, new object[] { content, timeout, onFinish });
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
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZGame;
using ZGame.Config;
using ZGame.Notify;

namespace ZGame.UI
{
    /// <summary>
    /// 消息弹窗
    /// </summary>
    [RefPath("Resources/MsgBox")]
    [UIOptions(UILayer.Notification, SceneType.Addition, CacheType.Permanent)]
    public class UIMsgBox : UIBase
    {
        private Action onYes;
        private Action onNo;


        public override void Enable(params object[] args)
        {
            base.Enable(args);
            string title = args[0].ToString();
            string content = args[1].ToString();
            this.onYes = (Action)(args[2]);
            this.onNo = (Action)(args[3]);

            ZG.Notify.Subscribe<KeyEventArgs>(KeyCode.Escape, OnBackup);
            TMP_Text[] texts = this.gameObject.GetComponentsInChildren<TMP_Text>(true);
            foreach (var VARIABLE in texts)
            {
                if (VARIABLE.name.Equals("title"))
                {
                    VARIABLE.SetText(title);
                }

                if (VARIABLE.name.Equals("content"))
                {
                    VARIABLE.SetText(content);
                }

                if (VARIABLE.name.Equals("text_yes"))
                {
                    VARIABLE.SetText(ZG.Language.Query("确定"));
                }

                if (VARIABLE.name.Equals("text_no"))
                {
                    VARIABLE.SetText(ZG.Language.Query("取消"));
                }
            }

            Button[] buttons = this.gameObject.GetComponentsInChildren<Button>(true);
            foreach (var VARIABLE in buttons)
            {
                if (VARIABLE.name.Equals("btn_yes"))
                {
                    VARIABLE.onClick.AddListener(() => Switch(true));
                }

                if (VARIABLE.name.Equals("btn_no"))
                {
                    VARIABLE.onClick.AddListener(() => Switch(false));
                }

                VARIABLE.gameObject.SetActive(onNo != null);
            }
        }

        private void OnBackup(KeyEventArgs args)
        {
            if (args.keyCode is not KeyCode.Escape && args.type is not KeyEventType.Down)
            {
                return;
            }

            Switch(false);
        }

        private void Switch(bool state)
        {
            ZG.Notify.Unsubscribe<KeyEventArgs>(KeyCode.Escape, OnBackup);
            ZG.UI?.Inactive(this);
            switch (state)
            {
                case true:
                    this.onYes?.Invoke();
                    break;
                case false:
                    this.onNo?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// 异步显示消息弹窗
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content">弹窗类容</param>
        /// <param name="onYes">点击确定按钮回调</param>
        /// <param name="onNo">点击取消按钮回调</param>
        public static void Show(string title, string content, Action onYes, Action onNo)
        {
            ZG.UI.Active<UIMsgBox>(new object[] { title, content, onYes, onNo });
        }

        /// <summary>
        /// 异步显示消息弹窗
        /// </summary>
        /// <param name="content">弹窗类容</param>
        /// <param name="onYes">点击确定按钮回调</param>
        /// <param name="onNo">点击取消按钮回调</param>
        public static void Show(string content, Action onYes, Action onNo)
        {
            Show(ZG.Language.Query("提示"), content, onYes, onNo);
        }

        /// <summary>
        /// 异步显示消息弹窗
        /// </summary>
        /// <param name="content">弹窗类容</param>
        /// <param name="onYes">点击确定按钮回调</param>
        public static void Show(string content, Action onYes)
        {
            Show(content, onYes, null);
        }

        /// <summary>
        /// 异步显示消息弹窗
        /// </summary>
        /// <param name="content">弹窗类容</param>
        public static void Show(string content)
        {
            Show(content, null);
        }

        /// <summary>
        /// 异步显示消息弹窗
        /// </summary>
        /// <param name="content">弹窗类容</param>
        /// <returns>点击的是确定还是取消按钮</returns>
        public static UniTask<bool> ShowAsync(string content, bool isNo = false)
        {
            return ShowAsync(ZG.Language.Query("提示"), content, isNo);
        }

        /// <summary>
        /// 异步显示消息弹窗
        /// </summary>
        /// <param name="title">弹窗标题</param>
        /// <param name="content">弹窗类容</param>
        /// <param name="isNo">是否显示取消按钮</param>
        /// <returns>点击的是确定还是取消按钮</returns>
        public static UniTask<bool> ShowAsync(string title, string content, bool isNo = false)
        {
            UniTaskCompletionSource<bool> tcs = new UniTaskCompletionSource<bool>();
            Action onYes = () => tcs.TrySetResult(true);
            Action onNo = isNo ? () => tcs.TrySetResult(false) : null;
            Show(title, content, onYes, onNo);
            return tcs.Task;
        }
    }
}
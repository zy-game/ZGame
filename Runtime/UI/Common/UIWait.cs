using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using ZGame;

namespace ZGame.UI
{
    /// <summary>
    /// 等待界面
    /// </summary>
    [RefPath("Resources/Waiting")]
    public sealed class UIWait : UIBase
    {
        private string title;
        private float timeout;

        public override void Start(params object[] args)
        {
            this.title = args[0].ToString();
            this.timeout = (float)args[1];
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

            if (timeout == 0)
            {
                return;
            }

            await UniTask.Delay((int)(timeout * 1000));
            Hide();
        }

        /// <summary>
        /// 显示等待界面
        /// </summary>
        /// <param name="s">显示内容</param>
        /// <param name="timeout">超时时长，如果超时为0，则一直显示，直到调用<see cref="Hide"/></param>
        public static void Show(string s, float timeout = 0)
        {
            AppCore.UI.Show<UIWait>(UILayer.Notification, new object[] { s, timeout });
        }

        /// <summary>
        /// 隐藏等待窗口
        /// </summary>
        public static void Hide()
        {
            AppCore.UI.Close<UIWait>();
        }
    }
}
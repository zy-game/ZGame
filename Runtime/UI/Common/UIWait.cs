using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using ZGame;

namespace ZGame.UI
{
    [ResourceReference("Resources/Waiting")]
    [UIOptions(UILayer.Notification, SceneType.Addition, CacheType.Permanent)]
    public sealed class UIWait : UIBase
    {
        public UIWait(GameObject gameObject) : base(gameObject)
        {
        }

        public override async void Enable(params object[] args)
        {
            base.Enable();
            TMP_Text[] texts = this.gameObject.GetComponentsInChildren<TMP_Text>();
            foreach (var VARIABLE in texts)
            {
                if (VARIABLE.name.Equals("content") is false)
                {
                    continue;
                }

                VARIABLE.SetText(args[0].ToString());
            }

            float time = (float)args[1];
            if (time == 0)
            {
                return;
            }

            await UniTask.Delay((int)(time * 1000));
            Hide();
        }

        public static void Show(string s, float timeout = 0)
        {
            GameFrameworkEntry.UI.Active<UIWait>(new object[] { s, timeout });
        }

        public static void Hide()
        {
            GameFrameworkEntry.UI.Inactive<UIWait>();
        }
    }
}
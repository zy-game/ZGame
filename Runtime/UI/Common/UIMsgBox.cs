using System;
using UnityEngine;
using ZGame.Window;

namespace UI
{
    public interface UIMsgBox : UIBase
    {
        void Setup(string title, string content, Action onYes, Action onNo);

        public static UIMsgBox Show(string title, string content, Action onYes, Action onNo)
        {
            UIMsgBox uiMsgBox = UIManager.instance.Open<UIMsgBox>();
            if (uiMsgBox is null)
            {
                Debug.Log("??");
                return default;
            }

            uiMsgBox.Setup(title, content, onYes, onNo);
            return uiMsgBox;
        }

        public static UIMsgBox Show(string content, Action onYes, Action onNo)
        {
            return Show("Tips", content, onYes, onNo);
        }

        public static UIMsgBox Show(string content, Action onYes)
        {
            return Show("Tips", content, onYes, null);
        }

        public static UIMsgBox Show(string content)
        {
            return Show("Tips", content, null, null);
        }
    }
}
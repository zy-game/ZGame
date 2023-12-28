using System;
using UnityEngine;
using ZGame.Window;

namespace UI
{
    public interface IMsgBox : UIBase
    {
        void Setup(string title, string content, Action onYes, Action onNo);

        public static IMsgBox Show(string title, string content, Action onYes, Action onNo)
        {
            IMsgBox msgBox = UIManager.instance.Open<IMsgBox>();
            if (msgBox is null)
            {
                Debug.Log("??");
                return default;
            }

            msgBox.Setup(title, content, onYes, onNo);
            return msgBox;
        }

        public static IMsgBox Show(string content, Action onYes, Action onNo)
        {
            return Show("Tips", content, onYes, onNo);
        }

        public static IMsgBox Show(string content, Action onYes)
        {
            return Show("Tips", content, onYes, null);
        }

        public static IMsgBox Show(string content)
        {
            return Show("Tips", content, null, null);
        }
    }
}
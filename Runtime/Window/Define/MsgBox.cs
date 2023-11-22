using System;
using UnityEngine;

namespace ZGame.Window
{
    [ResourceReference("Resources/Prefabs/MsgBox", 999)]
    public class MsgBox : UIBase
    {
        public MsgBox(GameObject gameObject) : base(gameObject)
        {
        }

        public MsgBox Setup(string title, string message, string okString, Action ok, string cancelString = "", Action cancel = null)
        {
            return this;
        }

        public static MsgBox Create(string message)
            => Create(message, () => { });

        public static MsgBox Create(string message, Action ok, Action cancel = null)
            => Create("Tips", message, ok, cancel);

        public static MsgBox Create(string title, string message, Action ok, Action cancel = null)
            => Create(title, message, "OK", ok, "Cancel", cancel);

        public static MsgBox Create(string title, string message, string okString, Action ok, string cancelString, Action cancel = null)
            => UIManager.instance.Open<MsgBox>().Setup(title, message, okString, ok, cancelString, cancel);
    }
}
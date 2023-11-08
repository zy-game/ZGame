using System;
using UnityEngine;

namespace ZGame.Window
{
    [Linked("Resources/MsgBox", 999)]
    public class MsgBox : GameWindow
    {
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
            => Engine.Window.Open<MsgBox>().Setup(title, message, okString, ok, cancelString, cancel);
    }
}
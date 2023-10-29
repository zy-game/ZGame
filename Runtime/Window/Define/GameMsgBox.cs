using System;
using UnityEngine;

namespace ZGame.Window
{
    [PrefabReference("Resources/MsgBox", 999)]
    public class GameMsgBox : GameWindow
    {
        public GameMsgBox Setup(string title, string message, string okString, Action ok, string cancelString = "", Action cancel = null)
        {
            return this;
        }

        public static GameMsgBox Create(string message)
            => Create(message, () => { });

        public static GameMsgBox Create(string message, Action ok, Action cancel = null)
            => Create("Tips", message, ok, cancel);

        public static GameMsgBox Create(string title, string message, Action ok, Action cancel = null)
            => Create(title, message, "OK", ok, "Cancel", cancel);

        public static GameMsgBox Create(string title, string message, string okString, Action ok, string cancelString, Action cancel = null)
            => CoreApi.Window.Open<GameMsgBox>().Setup(title, message, okString, ok, cancelString, cancel);
    }
}
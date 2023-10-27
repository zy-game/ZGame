using System;
using UnityEngine;

namespace ZGame.Window
{
    public interface IMsgBox : IGameWindow
    {
        IMsgBox Setup(string title, string message, string okString, Action ok, string cancelString = "", Action cancel = null);

        public static IMsgBox Create(string message)
            => Create(message, () => { });

        public static IMsgBox Create(string message, Action ok, Action cancel = null)
            => Create("Tips", message, ok, cancel);

        public static IMsgBox Create(string title, string message, Action ok, Action cancel = null)
            => Create(title, message, "OK", ok, "Cancel", cancel);

        public static IMsgBox Create(string title, string message, string okString, Action ok, string cancelString, Action cancel = null)
            => CoreApi.windowSystem.Open<DefaultMsgBoxer>().Setup(title, message, okString, ok, cancelString, cancel);

        class DefaultMsgBoxer : IMsgBox
        {
            public string guid { get; } = ID.New();
            public string name { get; }
            public GameObject gameObject { get; }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public IMsgBox Setup(string title, string message, string okString, Action ok, string cancelString = "", Action cancel = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
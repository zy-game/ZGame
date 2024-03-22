using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ZGame.Game
{
    public class KeyboardManager : GameFrameworkModule
    {
        class KeyHandle : IDisposable
        {
            public KeyCode keyCode;
            private UnityEvent callBack = new UnityEvent();

            public KeyHandle(KeyCode code)
            {
                keyCode = code;
            }

            public void OnTrigger()
            {
                callBack.Invoke();
            }

            public void Add(UnityAction action)
            {
                callBack.AddListener(action);
            }

            public void Remove(UnityAction action)
            {
                callBack.RemoveListener(action);
            }

            public void Dispose()
            {
                callBack.RemoveAllListeners();
                callBack = null;
            }
        }

        private bool isTouch = false;

        private List<KeyHandle> keyDownEvent = new List<KeyHandle>();
        private List<KeyHandle> keyUpEvent = new List<KeyHandle>();
        private List<KeyHandle> keyPressEvent = new List<KeyHandle>();

        public void SubscribeKeyDown(KeyCode code, UnityAction action)
        {
            KeyHandle handle = keyDownEvent.Find(x => x.keyCode == code);
            if (handle is null)
            {
                keyDownEvent.Add(handle = new KeyHandle(code));
            }

            handle.Add(action);
        }

        public void SubscribeKeyUp(KeyCode code, UnityAction action)
        {
            KeyHandle handle = keyUpEvent.Find(x => x.keyCode == code);
            if (handle is null)
            {
                keyUpEvent.Add(handle = new KeyHandle(code));
            }

            handle.Add(action);
        }

        public void SubscribeKeyPress(KeyCode code, UnityAction action)
        {
            KeyHandle handle = keyPressEvent.Find(x => x.keyCode == code);
            if (handle is null)
            {
                keyPressEvent.Add(handle = new KeyHandle(code));
            }

            handle.Add(action);
        }

        public void UnsubscribeKeyDown(KeyCode code, UnityAction action)
        {
            KeyHandle handle = keyDownEvent.Find(x => x.keyCode == code);
            if (handle is null)
            {
                return;
            }

            handle.Remove(action);
        }

        public void UnsubscribeKeyUp(KeyCode code, UnityAction action)
        {
            KeyHandle handle = keyUpEvent.Find(x => x.keyCode == code);
            if (handle is null)
            {
                return;
            }

            handle.Remove(action);
        }

        public void UnsubscribeKeyPress(KeyCode code, UnityAction action)
        {
            KeyHandle handle = keyPressEvent.Find(x => x.keyCode == code);
            if (handle is null)
            {
                return;
            }

            handle.Remove(action);
        }

        public override void Update()
        {
            CheckKeyDown();
            CheckKeyUp();
            CheckPressKey();
        }

        private void CheckKeyDown()
        {
            for (int i = 0; i < keyDownEvent.Count; i++)
            {
                if ((IsTouch(keyDownEvent[i].keyCode) && isTouch is false) || Input.GetKeyDown(keyDownEvent[i].keyCode))
                {
                    isTouch = IsMouse(keyDownEvent[i].keyCode);
                    keyDownEvent[i].OnTrigger();
                }
            }
        }

        private void CheckKeyUp()
        {
            for (int i = 0; i < keyUpEvent.Count; i++)
            {
                if ((IsTouch(keyUpEvent[i].keyCode) && isTouch) || Input.GetKeyUp(keyUpEvent[i].keyCode))
                {
                    isTouch = false;
                    keyUpEvent[i].OnTrigger();
                }
            }
        }

        private void CheckPressKey()
        {
            for (var i = 0; i < keyPressEvent.Count; i++)
            {
                if ((IsTouch(keyPressEvent[i].keyCode) && isTouch) || Input.GetKey(keyPressEvent[i].keyCode))
                {
                    keyPressEvent[i].OnTrigger();
                }
            }
        }

        private bool IsMouse(KeyCode keyCode)
        {
            return keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6;
        }

        private bool IsTouch(KeyCode keyCode)
        {
            return IsMouse(keyCode) && Input.touchCount > 0;
        }
    }
}
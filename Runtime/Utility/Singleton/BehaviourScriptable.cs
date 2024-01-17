using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ZGame
{
    public class KeyEvent : IDisposable
    {
        public KeyCode keyCode { get; }
        private bool isUsed { get; set; }
        private List<UnityAction<KeyEvent>> action { get; }

        public KeyEvent(KeyCode key)
        {
            this.keyCode = key;
            this.action = new List<UnityAction<KeyEvent>>();
        }

        public void Invoke()
        {
            if (this.action is null || this.action.Count == 0)
            {
                return;
            }

            isUsed = false;
            for (int i = this.action.Count - 1; i >= 0; i--)
            {
                if (isUsed)
                {
                    return;
                }

                this.action[i].Invoke(this);
            }
        }

        public void AddListener(UnityAction<KeyEvent> action)
        {
            this.action.Add(action);
        }

        public void RemoveListener(UnityAction<KeyEvent> action)
        {
            this.action.Remove(action);
        }

        public void Use()
        {
            this.isUsed = true;
        }

        public void Dispose()
        {
            this.action.Clear();
        }
    }

    class BehaviourScriptable : MonoBehaviour
    {
        UnityEvent onGUI = new UnityEvent();
        UnityEvent update = new UnityEvent();
        UnityEvent onDestroy = new UnityEvent();
        UnityEvent lateUpdate = new UnityEvent();
        UnityEvent fixedUpdate = new UnityEvent();
        UnityEvent onApplicationQuit = new UnityEvent();
        List<KeyEvent> keyUpEvent = new List<KeyEvent>();
        List<KeyEvent> keyDownEvent = new List<KeyEvent>();
        UnityEvent<bool> onApplicationPause = new UnityEvent<bool>();
        UnityEvent<bool> onApplicationFocus = new UnityEvent<bool>();


        private static BehaviourScriptable _instance;

        public static BehaviourScriptable instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("BevaviourScriptable").AddComponent<BehaviourScriptable>();
                    GameObject.DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        public void SetupKeyDown(KeyCode keyCode, UnityAction<KeyEvent> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEvent keyEvent = keyDownEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                keyDownEvent.Add(keyEvent = new KeyEvent(keyCode));
            }

            keyEvent.AddListener(action);
        }

        public void UnsetupKeyDown(KeyCode keyCode, UnityAction<KeyEvent> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEvent keyEvent = keyDownEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyEvent.RemoveListener(action);
        }

        public void SetupKeyUp(KeyCode keyCode, UnityAction<KeyEvent> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEvent keyEvent = keyUpEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                keyUpEvent.Add(keyEvent = new KeyEvent(keyCode));
            }

            keyEvent.AddListener(action);
        }

        public void UnsetupKeyUp(KeyCode keyCode, UnityAction<KeyEvent> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEvent keyEvent = keyUpEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyEvent.RemoveListener(action);
        }

        public void ClearKeyDownEvent(KeyCode keyCode)
        {
            KeyEvent keyEvent = keyDownEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyDownEvent.Remove(keyEvent);
        }

        public void ClearKeyUpEvent(KeyCode keyCode)
        {
            KeyEvent keyEvent = keyUpEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyUpEvent.Remove(keyEvent);
        }

        public void SetupUpdate(UnityAction action)
        {
            update.AddListener(action);
        }

        public void UnsetupUpdate(UnityAction action)
        {
            update.RemoveListener(action);
        }

        public void SetupFixedUpdate(UnityAction action)
        {
            fixedUpdate.AddListener(action);
        }

        public void UnsetupFixedUpdate(UnityAction action)
        {
            fixedUpdate.RemoveListener(action);
        }

        public void SetupLateUpdate(UnityAction action)
        {
            lateUpdate.AddListener(action);
        }

        public void UnsetupLateUpdate(UnityAction action)
        {
            lateUpdate.RemoveListener(action);
        }

        public void SetupOnGUI(UnityAction action)
        {
            onGUI.AddListener(action);
        }

        public void UnsetupOnGUI(UnityAction action)
        {
            onGUI.RemoveListener(action);
        }

        public void SetupOnApplicationQuit(UnityAction action)
        {
            onApplicationQuit.AddListener(action);
        }

        public void UnsetupOnApplicationQuit(UnityAction action)
        {
            onApplicationQuit.RemoveListener(action);
        }

        public void SetupOnApplicationPause(UnityAction<bool> action)
        {
            onApplicationPause.AddListener(action);
        }

        public void UnsetupOnApplicationPause(UnityAction<bool> action)
        {
            onApplicationPause.RemoveListener(action);
        }

        public void SetupOnApplicationFocus(UnityAction<bool> action)
        {
            onApplicationFocus.AddListener(action);
        }

        public void UnsetupOnApplicationFocus(UnityAction<bool> action)
        {
            onApplicationFocus.RemoveListener(action);
        }

        public void SetupOnDestroy(UnityAction action)
        {
            onDestroy.AddListener(action);
        }

        public void UnsetupOnDestroy(UnityAction action)
        {
            onDestroy.RemoveListener(action);
        }

        public void ListenerDestroy(GameObject gameObject, UnityAction callback)
        {
            if (gameObject == null)
            {
                return;
            }

            BehaviourScriptable bevaviour = gameObject.GetComponent<BehaviourScriptable>();
            if (bevaviour == null)
            {
                bevaviour = gameObject.AddComponent<BehaviourScriptable>();
            }

            bevaviour.SetupOnDestroy(callback);
        }

        private void Update()
        {
            for (int i = 0; i < keyDownEvent.Count; i++)
            {
                if (Input.GetKeyDown(keyDownEvent[i].keyCode) is false)
                {
                    continue;
                }

                keyDownEvent[i].Invoke();
            }

            for (int i = 0; i < keyUpEvent.Count; i++)
            {
                if (Input.GetKeyUp(keyUpEvent[i].keyCode) is false)
                {
                    continue;
                }

                keyUpEvent[i].Invoke();
            }

            update.Invoke();
        }

        private void FixedUpdate()
        {
            fixedUpdate.Invoke();
        }

        private void LateUpdate()
        {
            lateUpdate.Invoke();
        }

        private void OnGUI()
        {
            onGUI.Invoke();
        }

        private void OnDestroy()
        {
            onDestroy.Invoke();
        }

        private void OnApplicationQuit()
        {
            onApplicationQuit.Invoke();
        }

        private void OnApplicationPause(bool pause)
        {
            onApplicationPause.Invoke(pause);
        }

        private void OnApplicationFocus(bool focus)
        {
            onApplicationFocus.Invoke(focus);
        }
    }
}
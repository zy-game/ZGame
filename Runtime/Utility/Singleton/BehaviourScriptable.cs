using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZGame.Config;
using ZGame.Game;
using ZGame.UI;

namespace ZGame
{
    /// <summary>
    /// Unity单列对象
    /// </summary>
    public class BehaviourScriptable : MonoBehaviour
    {
        private bool isTouch = false;
        UnityEvent onGUI = new UnityEvent();
        UnityEvent update = new UnityEvent();
        UnityEvent onDestroy = new UnityEvent();
        UnityEvent lateUpdate = new UnityEvent();
        UnityEvent fixedUpdate = new UnityEvent();
        UnityEvent onApplicationQuit = new UnityEvent();
        UnityEvent<bool> onApplicationPause = new UnityEvent<bool>();
        UnityEvent<bool> onApplicationFocus = new UnityEvent<bool>();

        List<KeyEventData> keyUpEvent = new List<KeyEventData>();
        List<KeyEventData> keyDownEvent = new List<KeyEventData>();
        List<KeyEventData> keyPressEvent = new List<KeyEventData>();

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

        private void OnBecameInvisible()
        {
            throw new NotImplementedException();
        }
        
        private void OnBecameVisible()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 设置按键长按事件
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="action"></param>
        public void SetupPressKeyEvent(KeyCode keyCode, UnityAction<KeyEventData> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEventData keyEvent = keyDownEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                keyDownEvent.Add(keyEvent = new KeyEventData(keyCode));
            }

            keyEvent.AddListener(action);
        }

        /// <summary>
        /// 移除按键长按事件
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="action"></param>
        public void UnsetupPressKeyEvent(KeyCode keyCode, UnityAction<KeyEventData> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEventData keyEvent = keyDownEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyEvent.RemoveListener(action);
        }

        /// <summary>
        /// 设置按键按下事件
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="action"></param>
        public void SetupKeyDownEvent(KeyCode keyCode, UnityAction<KeyEventData> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEventData keyEvent = keyDownEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                keyDownEvent.Add(keyEvent = new KeyEventData(keyCode));
            }

            keyEvent.AddListener(action);
        }

        /// <summary>
        /// 移除按键按下事件
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="action"></param>
        public void UnsetupKeyDownEvent(KeyCode keyCode, UnityAction<KeyEventData> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEventData keyEvent = keyDownEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyEvent.RemoveListener(action);
        }

        /// <summary>
        /// 设置按键抬起事件
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="action"></param>
        public void SetupKeyUpEvent(KeyCode keyCode, UnityAction<KeyEventData> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEventData keyEvent = keyUpEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                keyUpEvent.Add(keyEvent = new KeyEventData(keyCode));
            }

            keyEvent.AddListener(action);
        }

        /// <summary>
        /// 移除按键抬起事件
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="action"></param>
        public void UnsetupKeyUpEvent(KeyCode keyCode, UnityAction<KeyEventData> action)
        {
            if (action is null)
            {
                return;
            }

            KeyEventData keyEvent = keyUpEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyEvent.RemoveListener(action);
        }

        /// <summary>
        /// 清除按键按下事件
        /// </summary>
        /// <param name="keyCode"></param>
        public void ClearKeyDownEvent(KeyCode keyCode)
        {
            KeyEventData keyEvent = keyDownEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyDownEvent.Remove(keyEvent);
        }

        /// <summary>
        /// 清理按键抬起事件
        /// </summary>
        /// <param name="keyCode"></param>
        public void ClearKeyUpEvent(KeyCode keyCode)
        {
            KeyEventData keyEvent = keyUpEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyUpEvent.Remove(keyEvent);
        }

        /// <summary>
        /// 清理按键长按事件
        /// </summary>
        /// <param name="keyCode"></param>
        public void ClearKeyPressEvent(KeyCode keyCode)
        {
            KeyEventData keyEvent = keyPressEvent.Find(x => x.keyCode == keyCode);
            if (keyEvent is null)
            {
                return;
            }

            keyPressEvent.Remove(keyEvent);
        }

        /// <summary>
        /// 设置轮询事件
        /// </summary>
        /// <param name="action"></param>
        public void SetupUpdateEvent(UnityAction action)
        {
            update.AddListener(action);
        }

        /// <summary>
        /// 取消轮询事件
        /// </summary>
        /// <param name="action"></param>
        public void UnsetupUpdateEvent(UnityAction action)
        {
            update.RemoveListener(action);
        }

        /// <summary>
        /// 设置帧更新事件
        /// </summary>
        /// <param name="action"></param>
        public void SetupFixedUpdateEvent(UnityAction action)
        {
            fixedUpdate.AddListener(action);
        }

        /// <summary>
        /// 取消帧更新事件
        /// </summary>
        /// <param name="action"></param>
        public void UnsetupFixedUpdateEvent(UnityAction action)
        {
            fixedUpdate.RemoveListener(action);
        }

        /// <summary>
        /// 设置帧末事件
        /// </summary>
        /// <param name="action"></param>
        public void SetupLateUpdateEvent(UnityAction action)
        {
            lateUpdate.AddListener(action);
        }

        /// <summary>
        /// 取消帧末事件
        /// </summary>
        /// <param name="action"></param>
        public void UnsetupLateUpdateEvent(UnityAction action)
        {
            lateUpdate.RemoveListener(action);
        }

        /// <summary>
        /// 设置GUI绘制事件
        /// </summary>
        /// <param name="action"></param>
        public void SetupGUIDrawingEvent(UnityAction action)
        {
            onGUI.AddListener(action);
        }

        /// <summary>
        /// 取消GUI绘制事件
        /// </summary>
        /// <param name="action"></param>
        public void UnsetupGUIDrawingEvent(UnityAction action)
        {
            onGUI.RemoveListener(action);
        }

        /// <summary>
        /// 设置应用退出事件
        /// </summary>
        /// <param name="action"></param>
        public void SetupApplicationQuitEvent(UnityAction action)
        {
            onApplicationQuit.AddListener(action);
        }

        /// <summary>
        /// 取消应用退出事件
        /// </summary>
        /// <param name="action"></param>
        public void UnsetupApplicationQuitEvent(UnityAction action)
        {
            onApplicationQuit.RemoveListener(action);
        }

        /// <summary>
        /// 设置应用暂停事件
        /// </summary>
        /// <param name="action"></param>
        public void SetupApplicationPauseEvent(UnityAction<bool> action)
        {
            onApplicationPause.AddListener(action);
        }

        /// <summary>
        /// 取消应用暂停事件
        /// </summary>
        /// <param name="action"></param>
        public void UnsetupApplicationPauseEvent(UnityAction<bool> action)
        {
            onApplicationPause.RemoveListener(action);
        }

        /// <summary>
        /// 设置应用焦点事件
        /// </summary>
        /// <param name="action"></param>
        public void SetupApplicationFocusChangeEvent(UnityAction<bool> action)
        {
            onApplicationFocus.AddListener(action);
        }

        /// <summary>
        /// 取消应用焦点事件
        /// </summary>
        /// <param name="action"></param>
        public void UnsetupApplicationFocusChangeEvent(UnityAction<bool> action)
        {
            onApplicationFocus.RemoveListener(action);
        }

        /// <summary>
        /// 设置游戏对象销毁事件
        /// </summary>
        /// <param name="action"></param>
        public void SetupGameObjectDestroyEvent(UnityAction action)
        {
            onDestroy.AddListener(action);
        }

        /// <summary>
        /// 取消游戏对象销毁事件
        /// </summary>
        /// <param name="action"></param>
        public void UnsetupGameObjectDestroyEvent(UnityAction action)
        {
            onDestroy.RemoveListener(action);
        }

        private void Awake()
        {
            SetupKeyDownEvent(KeyCode.Escape, keyEvent => { UIMsgBox.Show(WorkApi.Language.Query("是否退出"), WorkApi.Uninitialized); });
        }

        private void Update()
        {
            CheckKeyDown();
            CheckKeyUp();
            CheckPressKey();
            update.Invoke();
        }

        private void CheckKeyDown()
        {
            for (int i = 0; i < keyDownEvent.Count; i++)
            {
                if ((IsTouch(keyDownEvent[i].keyCode) && isTouch is false) || Input.GetKeyDown(keyDownEvent[i].keyCode))
                {
                    isTouch = IsMouse(keyDownEvent[i].keyCode);
                    keyDownEvent[i].Invoke();
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
                    keyUpEvent[i].Invoke();
                }
            }
        }

        private void CheckPressKey()
        {
            for (var i = 0; i < keyPressEvent.Count; i++)
            {
                if ((IsTouch(keyPressEvent[i].keyCode) && isTouch) || Input.GetKey(keyPressEvent[i].keyCode))
                {
                    keyPressEvent[i].Invoke();
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
            keyUpEvent.ForEach(x => x.Dispose());
            keyDownEvent.ForEach(x => x.Dispose());
            keyPressEvent.ForEach(x => x.Dispose());
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
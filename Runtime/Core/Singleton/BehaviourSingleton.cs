using System;
using UnityEngine;
using UnityEngine.Events;

namespace ZEngine
{
    public class BehaviourSingleton : MonoBehaviour
    {
        public static BehaviourSingleton instance => SingletonHandle.GetInstance();

        internal class SingletonHandle
        {
            private static BehaviourSingleton _instance;
            private static GameObject gameObject;

            public static BehaviourSingleton GetInstance()
            {
                if (_instance is not null)
                {
                    return _instance;
                }

                gameObject = new GameObject(typeof(BehaviourSingleton).Name);
                GameObject.DontDestroyOnLoad(gameObject);
                _instance = gameObject.AddComponent<BehaviourSingleton>();
                return _instance;
            }
        }

        private UnityEvent applicationQuitCallback = new UnityEvent();
        private UnityEvent fixedUpdateCallback = new UnityEvent();
        private UnityEvent updateCallback = new UnityEvent();
        private UnityEvent lateUpdateCallback = new UnityEvent();
        private UnityEvent<bool> applicationFocus = new UnityEvent<bool>();
        private UnityEvent destroyGameObjectCallback = new UnityEvent();
        private UnityEvent gui = new UnityEvent();

        public static void OnGUICall(UnityAction action)
        {
            instance.gui.AddListener(action);
        }

        public static void OnRemoveOnGUICall(UnityAction action)
        {
            instance.gui.RemoveListener(action);
        }

        public static void OnDestroyGameObject(GameObject gameObject, UnityAction action)
        {
            gameObject.TryGetComponent<BehaviourSingleton>().destroyGameObjectCallback.AddListener(action);
        }

        public static void RemoveDestroyGameObject(GameObject gameObject, UnityAction action)
        {
            gameObject.TryGetComponent<BehaviourSingleton>().destroyGameObjectCallback.RemoveListener(action);
        }

        public static void OnUpdate(UnityAction action)
        {
            instance.updateCallback.AddListener(action);
        }

        public static void OnFixedUpdate(UnityAction action)
        {
            instance.fixedUpdateCallback.AddListener(action);
        }

        public static void OnLateUpdate(UnityAction action)
        {
            instance.lateUpdateCallback.AddListener(action);
        }

        public static void OnApplicationQuiting(UnityAction action)
        {
            instance.applicationQuitCallback.AddListener(action);
        }

        public static void OnApplicationFocusChange(UnityAction<bool> action)
        {
            instance.applicationFocus.AddListener(action);
        }

        public static void RemoveUpdate(UnityAction action)
        {
            instance.updateCallback.RemoveListener(action);
        }

        public static void RemoveFixedUpdate(UnityAction action)
        {
            instance.fixedUpdateCallback.RemoveListener(action);
        }

        public static void RemoveLateUpdate(UnityAction action)
        {
            instance.lateUpdateCallback.RemoveListener(action);
        }

        public static void RemoveApplicationQuit(UnityAction action)
        {
            instance.applicationQuitCallback.RemoveListener(action);
        }

        public static void RemoveApplicationFocus(UnityAction<bool> action)
        {
            instance.applicationFocus.RemoveListener(action);
        }

        private void OnGUI()
        {
            gui.Invoke();
        }

        private void OnDestroy()
        {
            destroyGameObjectCallback.Invoke();
        }

        private void LateUpdate()
        {
            lateUpdateCallback?.Invoke();
        }

        private void FixedUpdate()
        {
            fixedUpdateCallback?.Invoke();
        }

        private void Update()
        {
            updateCallback?.Invoke();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            applicationFocus?.Invoke(hasFocus);
        }

        private void OnApplicationQuit()
        {
            applicationQuitCallback?.Invoke();
        }
    }
}
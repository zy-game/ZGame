using UnityEngine;
using UnityEngine.Events;

namespace ZEngine
{
    public class BehaviourSingleton<T> : MonoBehaviour where T : BehaviourSingleton<T>, new()
    {
        public static T instance => SingletonHandle.GetInstance();

        internal class SingletonHandle
        {
            private static T _instance;
            private static GameObject gameObject;

            public static T GetInstance()
            {
                if (_instance is not null)
                {
                    return _instance;
                }

                gameObject = new GameObject(typeof(T).Name);
                GameObject.DontDestroyOnLoad(gameObject);
                _instance = (T)gameObject.AddComponent<BehaviourSingleton<T>>();
                return _instance;
            }
        }
    }

    public sealed class UnityBehaviour : BehaviourSingleton<UnityBehaviour>
    {
        private UnityEvent applicationQuitCallback = new UnityEvent();
        private UnityEvent fixedUpdateCallback = new UnityEvent();
        private UnityEvent updateCallback = new UnityEvent();
        private UnityEvent lateUpdateCallback = new UnityEvent();
        private UnityEvent<bool> applicationFocus = new UnityEvent<bool>();
        private UnityEvent destroyGameObjectCallback = new UnityEvent();

        public void OnDestroyGameObject(UnityAction action)
        {
            destroyGameObjectCallback.AddListener(action);
        }

        public void RemoveDestroyGameObject(UnityAction action)
        {
            destroyGameObjectCallback.RemoveListener(action);
        }

        public void OnUpdate(UnityAction action)
        {
            updateCallback.AddListener(action);
        }

        public void OnFixedUpdate(UnityAction action)
        {
            fixedUpdateCallback.AddListener(action);
        }

        public void OnLateUpdate(UnityAction action)
        {
            lateUpdateCallback.AddListener(action);
        }

        public void OnApplicationQuit(UnityAction action)
        {
            applicationQuitCallback.AddListener(action);
        }

        public void OnApplicationFocus(UnityAction<bool> action)
        {
            applicationFocus.AddListener(action);
        }

        public void RemoveUpdate(UnityAction action)
        {
            updateCallback.RemoveListener(action);
        }

        public void RemoveFixedUpdate(UnityAction action)
        {
            fixedUpdateCallback.RemoveListener(action);
        }

        public void RemoveLateUpdate(UnityAction action)
        {
            lateUpdateCallback.RemoveListener(action);
        }

        public void RemoveApplicationQuit(UnityAction action)
        {
            applicationQuitCallback.RemoveListener(action);
        }

        public void RemoveApplicationFocus(UnityAction<bool> action)
        {
            applicationFocus.RemoveListener(action);
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
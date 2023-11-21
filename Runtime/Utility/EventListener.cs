using UnityEngine;
using UnityEngine.Events;

namespace ZGame
{
    class EventListener : MonoBehaviour
    {
        [HideInInspector] public UnityEvent update = new UnityEvent();
        [HideInInspector] public UnityEvent fixedUpdate = new UnityEvent();
        [HideInInspector] public UnityEvent lateUpdate = new UnityEvent();
        [HideInInspector] public UnityEvent onGUI = new UnityEvent();
        [HideInInspector] public UnityEvent onApplicationQuit = new UnityEvent();
        [HideInInspector] public UnityEvent<bool> onApplicationPause = new UnityEvent<bool>();
        [HideInInspector] public UnityEvent<bool> onApplicationFocus = new UnityEvent<bool>();
        [HideInInspector] public UnityEvent onDestroy = new UnityEvent();


        private static EventListener _behaviour;

        public static EventListener instance
        {
            get
            {
                if (_behaviour == null)
                {
                    _behaviour = new GameObject("SINGLETON_BEHAVIOUR").AddComponent<EventListener>();
                    GameObject.DontDestroyOnLoad(_behaviour.gameObject);
                }

                return _behaviour;
            }
        }

        private void Update()
        {
            update?.Invoke();
        }

        private void FixedUpdate()
        {
            fixedUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            lateUpdate?.Invoke();
        }

        private void OnGUI()
        {
            onGUI?.Invoke();
        }

        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }

        private void OnApplicationQuit()
        {
            onApplicationQuit?.Invoke();
        }

        private void OnApplicationPause(bool pause)
        {
            onApplicationPause?.Invoke(pause);
        }

        private void OnApplicationFocus(bool focus)
        {
            onApplicationFocus?.Invoke(focus);
        }
    }
}
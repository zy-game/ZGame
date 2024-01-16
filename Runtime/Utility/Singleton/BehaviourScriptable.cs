using UnityEngine;
using UnityEngine.Events;

namespace ZGame
{
    class BehaviourScriptable : MonoBehaviour
    {
        UnityEvent update = new UnityEvent();
        UnityEvent fixedUpdate = new UnityEvent();
        UnityEvent lateUpdate = new UnityEvent();
        UnityEvent onGUI = new UnityEvent();
        UnityEvent onApplicationQuit = new UnityEvent();
        UnityEvent<bool> onApplicationPause = new UnityEvent<bool>();
        UnityEvent<bool> onApplicationFocus = new UnityEvent<bool>();
        UnityEvent onDestroy = new UnityEvent();

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

        private void Update()
        {
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
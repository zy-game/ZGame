using UnityEngine;
using UnityEngine.Events;

namespace ZEngine
{
    class UnityFunctionLinker : MonoBehaviour
    {
        public UnityEvent quit = new UnityEvent();
        public UnityEvent fixedEvent = new UnityEvent();
        public UnityEvent updateEvent = new UnityEvent();
        public UnityEvent lateEvent = new UnityEvent();
        public UnityEvent<bool> focusEvent = new UnityEvent<bool>();
        public UnityEvent destroyEvent = new UnityEvent();

        private static GameObject _gameObject;
        private static UnityFunctionLinker _linker;

        public static UnityFunctionLinker instance
        {
            get
            {
                if (_gameObject == null)
                {
                    _gameObject = new GameObject("ZEngine");
                    GameObject.DontDestroyOnLoad(_gameObject);
                    _linker = _gameObject.AddComponent<UnityFunctionLinker>();
                }

                return _linker;
            }
        }

        private void LateUpdate()
        {
            lateEvent?.Invoke();
        }

        private void FixedUpdate()
        {
            fixedEvent?.Invoke();
        }

        private void Update()
        {
            updateEvent?.Invoke();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            focusEvent?.Invoke(hasFocus);
        }

        private void OnApplicationQuit()
        {
            quit?.Invoke();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Window
{
    public interface IAsyncWindow : IReference
    {
        object result { get; }
        IEnumerator GetCoroutine();
    }

    public abstract class UIWindow : IReference
    {
        private Dictionary<string, GameObject> childList = new Dictionary<string, GameObject>();
        public GameObject gameObject { get; private set; }

        internal void SetGameObject(GameObject value)
        {
            this.gameObject = value;
            foreach (var VARIABLE in this.gameObject.GetComponentsInChildren<RectTransform>(true))
            {
                if (childList.ContainsKey(VARIABLE.name))
                {
                    continue;
                }

                childList.Add(VARIABLE.name, VARIABLE.gameObject);
            }
        }

        public void OnAwake()
        {
            Awake();
        }

        public void OnEnable()
        {
            gameObject.SetActive(true);
            Enable();
        }

        public void OnDiable()
        {
            gameObject.SetActive(false);
            Disable();
        }

        public GameObject GetChild(string name)
        {
            if (childList.TryGetValue(name, out GameObject gameObject))
            {
                return gameObject;
            }

            return default;
        }

        public void Release()
        {
            Destroy();
            GameObject.DestroyImmediate(gameObject);
            childList.Clear();
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Enable()
        {
        }

        protected virtual void Disable()
        {
        }

        protected virtual void Destroy()
        {
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine
{
    class RuntimeCacheing : Single<RuntimeCacheing>
    {
        internal CacheHandle handle;
        internal List<CacheTokenHandle> cacheList = new List<CacheTokenHandle>();

        public RuntimeCacheing()
        {
            handle = new GameObject("CacheHandle").AddComponent<CacheHandle>();
            GameObject.DontDestroyOnLoad(handle);
        }

        public CacheTokenHandle Enqueue(object value)
        {
            CacheTokenHandle tokenHandle = new CacheTokenHandle();
            tokenHandle.Initialized(value);
            cacheList.Add(tokenHandle);
            return tokenHandle;
        }

        public T Dequeue<T>(CacheTokenHandle tokenHandle)
        {
            CacheTokenHandle temp = (CacheTokenHandle)cacheList.Find(x => x.Equals(tokenHandle));
            if (temp.value is null)
            {
                return default;
            }

            T result = (T)temp.value;
            cacheList.Remove(temp);
            return result;
        }

        internal class CacheHandle : MonoBehaviour
        {
            private Action OnUpdateCallback;

            public void AddUpdate(Action callback)
            {
                OnUpdateCallback += callback;
            }

            public void RemoveUpdate(Action callback)
            {
                OnUpdateCallback -= callback;
            }

            private void Update()
            {
                OnUpdateCallback?.Invoke();
            }
        }
    }
}
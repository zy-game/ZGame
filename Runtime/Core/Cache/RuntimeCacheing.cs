using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Core
{
    [Serializable]
    public sealed class CacheOptions
    {
        [Header("缓存时间")] public float time;
    }

    public interface ICacheToken : IReference
    {
        void Restart();
    }

    public class RuntimeCacheing : Single<RuntimeCacheing>
    {
        private CacheHandle handle;
        private List<ICacheToken> cacheList = new List<ICacheToken>();

        public RuntimeCacheing()
        {
            handle = new GameObject("CacheHandle").AddComponent<CacheHandle>();
            GameObject.DontDestroyOnLoad(handle);
        }

        public ICacheToken Enqueue(object value)
        {
            CacheToken token = Engine.Reference.Dequeue<CacheToken>();
            token.Initialized(value);
            cacheList.Add(token);
            return token;
        }

        public T Dequeue<T>(ICacheToken token)
        {
            CacheToken temp = (CacheToken)cacheList.Find(x => x.Equals(token));
            if (temp is null)
            {
                return default;
            }

            T result = (T)temp.value;
            cacheList.Remove(temp);
            Engine.Reference.Release(temp);
            return result;
        }

        class CacheToken : ICacheToken
        {
            public object value;
            private float releaseTime;

            public void Initialized(object value)
            {
                this.value = value;
                releaseTime = Time.realtimeSinceStartup + AppConfig.instance.cacheOptions.time;
                instance.handle.AddUpdate(CheckTimeout);
            }

            private void CheckTimeout()
            {
                if (Time.realtimeSinceStartup < releaseTime)
                {
                    return;
                }

                if (value is IReference reference)
                {
                    Engine.Reference.Release(reference);
                }

                instance.cacheList.Remove(this);
                Engine.Reference.Release(this);
            }

            public void Release()
            {
                value = null;
                releaseTime = 0;
                instance.handle.RemoveUpdate(CheckTimeout);
            }

            public void Restart()
            {
                releaseTime = Time.realtimeSinceStartup + AppConfig.instance.cacheOptions.time;
            }
        }

        class CacheHandle : MonoBehaviour
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
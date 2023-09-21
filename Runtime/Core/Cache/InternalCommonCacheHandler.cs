using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Cache
{
    class InternalCommonCacheHandler : ICacheHandler
    {
        private List<Handle> list = new List<Handle>();
        public int count { get; }
        public Type cacheType { get; set; }

        class Handle : IDisposable
        {
            public string key;
            public object target;
            public float time;

            public void Dispose()
            {
                if (target is UnityEngine.Object gameObject)
                {
                    UnityEngine.Object.DestroyImmediate(gameObject);
                    Resources.UnloadUnusedAssets();
                }

                key = String.Empty;
                target = null;
                time = 0;
                GC.SuppressFinalize(this);
            }

            public static Handle Create(string key, object target)
            {
                Handle handle = Activator.CreateInstance<Handle>();
                handle.key = key;
                handle.target = target;
                handle.time = Time.realtimeSinceStartup + 60;
                return handle;
            }
        }

        public InternalCommonCacheHandler()
        {
            UnityEventArgs.Subscribe(GameEventType.Update, OnUpdate);
        }

        private void OnUpdate(UnityEventArgs obj)
        {
            for (int i = list.Count; i >= 0; i--)
            {
                if (Time.realtimeSinceStartup >= list[i].time)
                {
                    list[i].Dispose();
                    list.Remove(list[i]);
                }
            }
        }

        public void Release(string key, object value)
        {
            Handle temp = list.Find(x => x.key == key);
            if (temp is not null)
            {
                Engine.Console.Error("已存在相同名字的缓存对象:", key);
                return;
            }

            list.Add(Handle.Create(key, value));
        }

        public object Dequeue(string key)
        {
            if (count == 0)
            {
                return default;
            }

            Handle temp = list.Find(x => x.key == key);
            if (temp is null)
            {
                return default;
            }

            list.Remove(temp);
            object result = temp.target;
            temp.target = null;
            temp.Dispose();
            return result;
        }

        public void Dispose()
        {
            foreach (var VARIABLE in list)
            {
                VARIABLE.Dispose();
            }

            UnityEventArgs.Unsubscribe(GameEventType.Update, OnUpdate);
            list.Clear();
            GC.SuppressFinalize(this);
        }

        public static ICacheHandler Create(Type type)
        {
            InternalCommonCacheHandler internalCommonCacheHandler = Activator.CreateInstance<InternalCommonCacheHandler>();
            internalCommonCacheHandler.cacheType = type;
            return internalCommonCacheHandler;
        }
    }
}
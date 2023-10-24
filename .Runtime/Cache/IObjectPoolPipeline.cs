using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Cache
{
    public interface IObjectPoolPipeline : IDisposable
    {
        int count { get; }
        Type cacheType { get; }
        void Release(object key, object value);
        object Dequeue(object key);
        void OnUpdate();

        public static IObjectPoolPipeline Create(Type type)
        {
            ObjectPoolHandler objectHandler = Activator.CreateInstance<ObjectPoolHandler>();
            objectHandler.cacheType = type;
            return objectHandler;
        }

        class ObjectPoolHandler : IObjectPoolPipeline
        {
            public int count { get; }
            public Type cacheType { get; set; }
            private List<Handle> list = new List<Handle>();

            class Handle : IDisposable
            {
                public object key;
                public object target;
                public float time;

                public void Dispose()
                {
                    if (target is UnityEngine.Object gameObject)
                    {
                        UnityEngine.Object.DestroyImmediate(gameObject);
                        Resources.UnloadUnusedAssets();
                    }

                    key = default;
                    target = null;
                    time = 0;
                    GC.SuppressFinalize(this);
                }

                public static Handle Create(object key, object target)
                {
                    Handle handle = Activator.CreateInstance<Handle>();
                    handle.key = key;
                    handle.target = target;
                    handle.time = Time.realtimeSinceStartup + 60;
                    return handle;
                }
            }

            public void OnUpdate()
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

            public void Release(object key, object value)
            {
                Handle temp = list.Find(x => x.key.Equals(key));
                if (temp is not null)
                {
                    ZGame.Console.Error("已存在相同名字的缓存对象:", key);
                    return;
                }

                list.Add(Handle.Create(key, value));
            }

            public object Dequeue(object key)
            {
                if (count == 0)
                {
                    return default;
                }

                Handle temp = list.Find(x => x.key.Equals(key));
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

                list.Clear();
                GC.SuppressFinalize(this);
            }
        }
    }
}
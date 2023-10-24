using System;
using System.Collections.Generic;
using System.Linq;

namespace ZEngine
{
    class DatableManager : Singleton<DatableManager>
    {
        private List<Handle> handles = new List<Handle>();

        class Handle : IDisposable
        {
            public object key;
            public object data;

            public void Dispose()
            {
                if (data is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                key = String.Empty;
                data = null;
            }
        }

        public void Add(object key, object data)
        {
            Handle handle = handles.Find(x => x.key == key);
            if (handle is not null)
            {
                ZGame.Console.Error("存在相同的键值数据");
                return;
            }

            handles.Add(new Handle()
            {
                key = key,
                data = data,
            });
        }

        public bool TryGetValue<T>(object key, out T runtimeDatableHandle)
        {
            Type type = typeof(T);
            Handle handle = handles.Find(x => x.key.Equals(key) );
            if (handle is null)
            {
                runtimeDatableHandle = default;
                return false;
            }

            runtimeDatableHandle = (T)handle.data;
            return runtimeDatableHandle != null;
        }

        public T GetDatable<T>(object key)
        {
            if (TryGetValue<T>(key, out T runtimeDatableHandle) is false)
            {
                return default;
            }

            return runtimeDatableHandle;
        }

        public T GetDatable<T>(Func<T, bool> func)
        {
            return (T)handles.Where(x => func((T)x.data)).FirstOrDefault()?.data;
        }

        public void Release(object key, bool isCache = false)
        {
            Handle handle = handles.Find(x => x.key == key);
            if (handle is null)
            {
                return;
            }

            handles.Remove(handle);
            if (isCache)
            {
                ZGame.Cache.Enqueue(handle.key, handle);
                return;
            }

            handle.Dispose();
        }

        public void Clear(Type type, bool isCache)
        {
            IEnumerable<Handle> list = type is null ? handles : handles.Where(x => x.GetType() == type);
            foreach (var VARIABLE in list)
            {
                handles.Remove(VARIABLE);
                if (isCache)
                {
                    ZGame.Cache.Enqueue(VARIABLE.key, VARIABLE);
                    continue;
                }

                VARIABLE.Dispose();
            }

            if (type is null)
            {
                handles.Clear();
            }
        }
    }
}
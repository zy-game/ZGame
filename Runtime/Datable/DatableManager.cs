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
            public Type type;
            public object key;
            public object data;

            public void Dispose()
            {
                if (data is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                type = null;
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
                type = data.GetType()
            });
        }

        public bool TryGetValue<T>(object key, out T runtimeDatableHandle)
        {
            Type type = typeof(T);
            Handle handle = handles.Find(x => x.key.Equals(key) && type == x.type);
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
            Type type = typeof(T);
            List<object> list = handles.Where(x => x.type == type).Select(x => x.data).ToList();
            foreach (var VARIABLE in list)
            {
                return (T)VARIABLE;
            }

            return default;
        }

        public List<T> GetDatables<T>()
        {
            Type type = typeof(T);
            List<object> list = handles.Where(x => x.type == type).Select(x => x.data).ToList();
            List<T> result = new List<T>();
            foreach (var VARIABLE in list)
            {
                result.Add((T)VARIABLE);
            }

            return result;
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
            IEnumerable<Handle> list = type is null ? handles : handles.Where(x => x.type == type);
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
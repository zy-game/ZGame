using System;
using System.Collections.Generic;
using System.Linq;

namespace ZEngine
{
    class DatableManager : Singleton<DatableManager>
    {
        private static Dictionary<Type, List<IDatableHandle>> _handles = new Dictionary<Type, List<IDatableHandle>>();

        public void Add<T>(T runtimeDatableHandle) where T : IDatableHandle
        {
            if (_handles.TryGetValue(typeof(T), out List<IDatableHandle> list) is false)
            {
                _handles.Add(typeof(T), list = new List<IDatableHandle>());
            }

            list.Add(runtimeDatableHandle);
        }

        public bool TryGetValue<T>(string name, out T runtimeDatableHandle)
        {
            if (_handles.TryGetValue(typeof(T), out List<IDatableHandle> list) is false)
            {
                runtimeDatableHandle = default;
                return false;
            }

            runtimeDatableHandle = (T)list.Find(x => x.name == name);
            return runtimeDatableHandle != null;
        }

        public T GetRuntimeDatableHandle<T>(string name) where T : IDatableHandle
        {
            if (TryGetValue<T>(name, out T runtimeDatableHandle) is false)
            {
                return default;
            }

            return runtimeDatableHandle;
        }

        public void Release<T>(string name, bool isCache = false) where T : IDatableHandle
        {
            Release(GetRuntimeDatableHandle<T>(name), isCache);
        }

        public void Release(IDatableHandle runtimeDatableHandle, bool isCache = false)
        {
            if (runtimeDatableHandle is null)
            {
                return;
            }

            if (_handles.TryGetValue(runtimeDatableHandle.GetType(), out List<IDatableHandle> list) is false)
            {
                return;
            }

            list.Remove(runtimeDatableHandle);
            if (isCache)
            {
                ZGame.Cache.Enqueue(runtimeDatableHandle.name, runtimeDatableHandle);
                return;
            }

            runtimeDatableHandle.Dispose();
        }

        public void Clear<T>(bool isCache) where T : IDatableHandle
        {
            if (_handles.TryGetValue(typeof(T), out List<IDatableHandle> list) is false)
            {
                return;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                IDatableHandle runtimeDatableHandle = list[i];
                if (isCache)
                {
                    ZGame.Cache.Enqueue(runtimeDatableHandle.name, runtimeDatableHandle);
                    continue;
                }

                runtimeDatableHandle.Dispose();
            }

            list.Clear();
        }

        public IDatableHandle Find(Func<IDatableHandle, bool> comper)
        {
            List<IDatableHandle> result = new List<IDatableHandle>();
            foreach (var VARIABLE in _handles.Values)
            {
                result.AddRange(VARIABLE.Where(comper));
            }

            return result.FirstOrDefault();
        }

        public IDatableHandle[] Where(Func<IDatableHandle, bool> comper)
        {
            List<IDatableHandle> result = new List<IDatableHandle>();
            foreach (var VARIABLE in _handles.Values)
            {
                result.AddRange(VARIABLE.Where(comper));
            }

            return result.ToArray();
        }

        public T Find<T>(Func<T, bool> comper)
        {
            if (_handles.TryGetValue(typeof(T), out List<IDatableHandle> list))
            {
                return (T)list.Find(x => comper((T)x));
            }

            return default;
        }

        public T[] Where<T>(Func<T, bool> comper)
        {
            List<IDatableHandle> temp = new List<IDatableHandle>();
            if (_handles.TryGetValue(typeof(T), out List<IDatableHandle> list))
            {
                temp.AddRange(list.Where(x => comper((T)x)));
            }

            T[] result = new T[temp.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (T)temp[i];
            }

            return result;
        }
    }
}
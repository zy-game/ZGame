using System;
using System.Collections.Generic;

namespace ZEngine.Cache
{
    public class ObjectManager : Singleton<ObjectManager>
    {
        private List<IObjectPoolHandle> handlers = new List<IObjectPoolHandle>();

        protected override void OnUpdate()
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].OnUpdate();
            }
        }

        public void SetCacheHandle(Type handleType)
        {
            if (handlers.Find(x => x.GetType() == handleType) is not null)
            {
                ZGame.Console.Error("已经存在了相同的缓存管道");
                return;
            }

            handlers.Add((IObjectPoolHandle)Activator.CreateInstance(handleType));
        }

        public void RemoveCacheHandle(Type handleType)
        {
            IObjectPoolHandle cacheHandler = handlers.Find(x => x.GetType() == handleType);
            if (cacheHandler is null)
            {
                return;
            }

            cacheHandler.Dispose();
            handlers.Remove(cacheHandler);
        }

        public void RemoveCacheArea(Type type)
        {
            IObjectPoolHandle cacheHandler = handlers.Find(x => x.cacheType == type);
            if (cacheHandler is null)
            {
                return;
            }

            cacheHandler.Dispose();
            handlers.Remove(cacheHandler);
        }

        public void Enqueue(string key, object value)
        {
            Type valueType = value.GetType();
            IObjectPoolHandle cacheHandler = handlers.Find(x => x.cacheType == valueType);
            if (cacheHandler is null)
            {
                cacheHandler = ObjectPoolHandler.Create(valueType);
            }

            cacheHandler.Release(key, value);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;
            Type valueType = typeof(T);
            IObjectPoolHandle cacheHandler = handlers.Find(x => x.cacheType == valueType);
            if (cacheHandler is null)
            {
                return false;
            }

            value = (T)cacheHandler.Dequeue(key);
            return value is not null;
        }
    }
}
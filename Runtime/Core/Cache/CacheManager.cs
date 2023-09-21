using System;
using System.Collections.Generic;

namespace ZEngine.Cache
{
    public class CacheManager : Single<CacheManager>
    {
        private List<ICacheHandler> handlers = new List<ICacheHandler>();

        public void SetCacheHandle(Type handleType)
        {
            if (handlers.Find(x => x.GetType() == handleType) is not null)
            {
                Engine.Console.Error("已经存在了相同的缓存管道");
                return;
            }

            handlers.Add((ICacheHandler)Activator.CreateInstance(handleType));
        }

        public void RemoveCacheHandle(Type handleType)
        {
            ICacheHandler cacheHandler = handlers.Find(x => x.GetType() == handleType);
            if (cacheHandler is null)
            {
                return;
            }

            cacheHandler.Dispose();
            handlers.Remove(cacheHandler);
        }

        public void Handle(string key, object value)
        {
            Type valueType = value.GetType();
            ICacheHandler cacheHandler = handlers.Find(x => x.cacheType == valueType);
            if (cacheHandler is null)
            {
                cacheHandler = InternalCommonCacheHandler.Create(valueType);
            }

            cacheHandler.Release(key, value);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;
            Type valueType = typeof(T);
            ICacheHandler cacheHandler = handlers.Find(x => x.cacheType == valueType);
            if (cacheHandler is null)
            {
                return false;
            }

            value = (T)cacheHandler.Dequeue(key);
            return value is not null;
        }
    }
}
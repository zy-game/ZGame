using System;
using System.Collections.Generic;

namespace ZEngine.Cache
{
    public class ObjectManager : Singleton<ObjectManager>
    {
        private List<IObjectPoolPipeline> handlers = new List<IObjectPoolPipeline>();
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

            handlers.Add((IObjectPoolPipeline)Activator.CreateInstance(handleType));
        }

        public void RemoveCacheHandle(Type handleType)
        {
            IObjectPoolPipeline cacheHandler = handlers.Find(x => x.GetType() == handleType);
            if (cacheHandler is null)
            {
                return;
            }

            cacheHandler.Dispose();
            handlers.Remove(cacheHandler);
        }

        public void RemoveCacheArea(Type type)
        {
            IObjectPoolPipeline cacheHandler = handlers.Find(x => x.cacheType == type);
            if (cacheHandler is null)
            {
                return;
            }

            cacheHandler.Dispose();
            handlers.Remove(cacheHandler);
        }

        public void Enqueue(object key, object value)
        {
            Type valueType = value.GetType();
            IObjectPoolPipeline cacheHandler = handlers.Find(x => x.cacheType == valueType);
            if (cacheHandler is null)
            {
                cacheHandler = IObjectPoolPipeline.Create(valueType);
            }

            cacheHandler.Release(key, value);
        }

        public bool TryGetValue<T>(object key, out T value)
        {
            value = default;
            Type valueType = typeof(T);
            IObjectPoolPipeline cacheHandler = handlers.Find(x => x.cacheType == valueType);
            if (cacheHandler is null)
            {
                return false;
            }

            value = (T)cacheHandler.Dequeue(key);
            return value is not null;
        }
    }
}
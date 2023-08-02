using UnityEngine;

namespace ZEngine
{
    public class CacheTokenHandle : IReference
    {
        internal object value;
        internal float releaseTime;
        public string name;

        internal void Initialized(object value)
        {
            this.value = value;
            releaseTime = Time.realtimeSinceStartup + CacheOptions.instance.time;
            RuntimeCacheing.instance.handle.AddUpdate(CheckTimeout);
        }

        private void CheckTimeout()
        {
            if (Time.realtimeSinceStartup < releaseTime)
            {
                return;
            }

            if (value is IReference reference)
            {
                Engine.Class.Release(reference);
            }

            RuntimeCacheing.instance.cacheList.Remove(this);
        }

        public void Release()
        {
            value = null;
            releaseTime = 0;
            RuntimeCacheing.instance.handle.RemoveUpdate(CheckTimeout);
        }

        public void Refresh()
        {
            releaseTime = Time.realtimeSinceStartup + CacheOptions.instance.time;
        }

        public static CacheTokenHandle Create(string name, object value)
        {
            CacheTokenHandle handle = new CacheTokenHandle();
            handle.value = value;
            handle.name = name;
            return handle;
        }
    }
}
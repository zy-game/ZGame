using UnityEngine;

namespace ZEngine
{
    public struct CacheTokenHandle
    {
        internal object value;
        internal float releaseTime;

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
    }
}
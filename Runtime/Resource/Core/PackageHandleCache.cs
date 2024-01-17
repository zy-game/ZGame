using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Resource
{
    class PackageHandleCache : Singleton<PackageHandleCache>
    {
        private float nextCheckTime;
        private List<PackageHandle> cacheList = new List<PackageHandle>();
        private List<PackageHandle> _packageList = new List<PackageHandle>();

        protected override void OnUpdate()
        {
            if (Time.realtimeSinceStartup < nextCheckTime)
            {
                return;
            }

            nextCheckTime = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
            CheckCanUnloadPackage();
            UnloadPackage();
        }

        private void CheckCanUnloadPackage()
        {
            PackageHandle handle = default;
            for (int i = _packageList.Count - 1; i >= 0; i--)
            {
                handle = _packageList[i];
                if (handle.CanUnloadPackage() is false)
                {
                    continue;
                }

                Debug.Log("资源包:" + handle.name + "准备卸载");
                cacheList.Add(handle);
                _packageList.Remove(handle);
            }
        }

        private void UnloadPackage()
        {
            for (int i = cacheList.Count - 1; i >= 0; i--)
            {
                if (cacheList[i].CanUnloadPackage() is false)
                {
                    continue;
                }

                Remove(cacheList[i].name);
            }
        }

        public void Add(PackageHandle handle)
        {
            _packageList.Add(handle);
        }

        public void Remove(string packageName)
        {
            PackageHandle handle = _packageList.Find(x => x.name == packageName);
            if (handle is not null)
            {
                _packageList.Remove(handle);
                handle.Dispose();
            }

            handle = cacheList.Find(x => x.name == packageName);
            if (handle is null)
            {
                return;
            }

            cacheList.Remove(handle);
            handle.Dispose();
        }

        public bool TryGetValue(string packageName, out PackageHandle handle)
        {
            handle = default;
            if ((handle = _packageList.Find(x => x.name == packageName)) is not null)
            {
                return true;
            }

            if ((handle = cacheList.Find(x => x.name == packageName)) is not null)
            {
                cacheList.Remove(handle);
                _packageList.Add(handle);
                return true;
            }

            return false;
        }
    }
}
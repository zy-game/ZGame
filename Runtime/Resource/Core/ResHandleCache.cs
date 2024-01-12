using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Resource
{
    class ResHandleCache : Singleton<ResHandleCache>
    {
        private List<ResHandle> cacheList;

        protected override void OnAwake()
        {
            cacheList = new List<ResHandle>();
        }

        public void Add(ResHandle handle)
        {
            cacheList.Add(handle);
        }

        public void Remove(ResHandle handle)
        {
            if (cacheList.Contains(handle) is false)
            {
                return;
            }

            cacheList.Remove(handle);
        }

        public void RemovePackage(string packageName)
        {
            for (int i = 0; i < cacheList.Count; i++)
            {
                if (cacheList[i].packageName.Equals(packageName) is false)
                {
                    continue;
                }

                Debug.Log("移除资源:" + cacheList[i].path);
                cacheList.Remove(cacheList[i]);
            }
        }

        public bool TryGetValue(string path, out ResHandle handle)
        {
            handle = cacheList.Find(x => x.path == path);
            return handle is not null;
        }

        public bool TryGetValue(string pacageName, string path, out ResHandle handle)
        {
            handle = cacheList.Find(x => x.path == path && x.packageName == pacageName);
            return handle is not null;
        }
    }
}
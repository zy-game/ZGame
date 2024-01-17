using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Resource
{
    class ResObjectCache : Singleton<ResObjectCache>
    {
        private List<ResObject> cacheList;

        protected override void OnAwake()
        {
            cacheList = new List<ResObject>();
        }

        public void Add(ResObject resObject)
        {
            cacheList.Add(resObject);
        }

        public void Remove(ResObject resObject)
        {
            if (cacheList.Contains(resObject) is false)
            {
                return;
            }

            cacheList.Remove(resObject);
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

        public bool TryGetValue(string path, out ResObject resObject)
        {
            resObject = cacheList.Find(x => x.path == path);
            return resObject is not null;
        }

        public bool TryGetValue(string pacageName, string path, out ResObject resObject)
        {
            resObject = cacheList.Find(x => x.path == path && x.packageName == pacageName);
            return resObject is not null;
        }
    }
}
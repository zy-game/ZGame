using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Resource
{
    class AssetBundleManager : Singleton<AssetBundleManager>
    {
        private List<AssetBundleHandle> _handles = new List<AssetBundleHandle>();

        internal AssetBundleHandle GetBundleHandle(AssetObjectHandle obj)
        {
            return _handles.Find(x => x.Contains(obj));
        }

        internal AssetBundleHandle GetBundleHandle(string path)
        {
            return _handles.Find(x => x.Contains(path));
        }

        internal void Remove(string name)
        {
            AssetBundleHandle handle = _handles.Find(x => x.name == name);
            if (handle is null)
            {
                return;
            }

            _handles.Remove(handle);
        }

        internal void Add(AssetBundle bundle)
        {
            _handles.Add(new AssetBundleHandle(bundle));
        }
    }
}
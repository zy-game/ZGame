using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Resource
{
    class BundleManager : Singleton<BundleManager>
    {
        private List<ResourcePackageHandle> _handles = new List<ResourcePackageHandle>();

        internal ResourcePackageHandle GetBundleHandle(ResHandle obj)
        {
            return _handles.Find(x => x.Contains(obj));
        }

        public ResourcePackageHandle GetABHandleWithName(string name)
        {
            return _handles.Find(x => x.name == name);
        }

        internal ResourcePackageHandle GetABHandleWithAssetPath(string path)
        {
            ResourcePackageManifest manifest = ResourcePackageListManifest.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                return default;
            }

            return GetABHandleWithName(manifest.name);
        }

        public bool Release(ResHandle handle)
        {
            ResourcePackageHandle _handle = _handles.Find(x => x.Contains(handle));
            if (_handle is null)
            {
                return false;
            }

            _handle.Release(handle);
            return true;
        }

        internal void Remove(string name)
        {
            ResourcePackageHandle handle = _handles.Find(x => x.name == name);
            if (handle is null)
            {
                return;
            }

            _handles.Remove(handle);
            handle.Dispose();
        }

        public ResourcePackageHandle Add(string title)
        {
            var m = new ResourcePackageHandle(title);
            _handles.Add(m);
            return m;
        }

        internal ResourcePackageHandle Add(AssetBundle bundle)
        {
            ResourcePackageHandle m = new ResourcePackageHandle(bundle);
            _handles.Add(m);
            return m;
        }
    }
}
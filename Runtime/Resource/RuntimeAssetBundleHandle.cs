using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    internal class RuntimeAssetBundleHandle : IReference
    {
        public string name;
        public int refCount;
        public string module;

        private AssetBundle bundle;
        private HashSet<Object> _handles;

        public void Release()
        {
            _handles.Clear();
            bundle.Unload(true);
            name = String.Empty;
            refCount = 0;
            module = String.Empty;
        }

        public bool Contains(Object target)
        {
            return _handles.Contains(target);
        }


        public T Load<T>(string path) where T : Object
        {
            return default;
        }

        public IEnumerator LoadAsync<T>(string path, ISubscribe<T> subscribe) where T : Object
        {
            yield break;
        }

        public void Unload(Object obj)
        {
            if (refCount == 0)
            {
                ResourceManager.instance.RemoveAssetBundleHandle(this);
            }
        }

        public static RuntimeAssetBundleHandle Create(BundleManifest manifest, AssetBundle bundle)
        {
            RuntimeAssetBundleHandle runtimeAssetBundleHandle = Engine.Class.Loader<RuntimeAssetBundleHandle>();
            runtimeAssetBundleHandle.bundle = bundle;
            runtimeAssetBundleHandle.name = manifest.name;
            runtimeAssetBundleHandle.module = manifest.owner;
            runtimeAssetBundleHandle.refCount = 0;
            runtimeAssetBundleHandle._handles = new HashSet<Object>();
            ResourceManager.instance.AddAssetBundleHandle(runtimeAssetBundleHandle);
            return runtimeAssetBundleHandle;
        }
    }
}
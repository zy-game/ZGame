using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface IRuntimeBundleHandle : IReference
    {
        string name { get; }
        uint refCount { get; }
        string module { get; }
        void Unload(Object obj);
        bool Contains(Object target);
        T Load<T>(string path) where T : Object;
        IEnumerator LoadAsync<T>(string path, ISubscribeExecuteHandle<T> subscribe) where T : Object;
    }

    internal class RuntimeAssetBundleHandle : IRuntimeBundleHandle
    {
        public string name { get; private set; }
        public uint refCount { get; private set; }
        public string module { get; private set; }

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

        public IEnumerator LoadAsync<T>(string path, ISubscribeExecuteHandle<T> subscribe) where T : Object
        {
            yield break;
        }

        public void Unload(Object obj)
        {
            refCount--;
        }

        public static RuntimeAssetBundleHandle Create(RuntimeBundleManifest manifest, AssetBundle bundle)
        {
            RuntimeAssetBundleHandle runtimeAssetBundleHandle = Engine.Class.Loader<RuntimeAssetBundleHandle>();
            runtimeAssetBundleHandle.bundle = bundle;
            runtimeAssetBundleHandle.name = manifest.name;
            runtimeAssetBundleHandle.module = manifest.owner;
            runtimeAssetBundleHandle.refCount = 0;
            runtimeAssetBundleHandle._handles = new HashSet<Object>();
            return runtimeAssetBundleHandle;
        }
    }
}
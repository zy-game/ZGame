﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    internal sealed class InternalRuntimeBundleHandle : IReference
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

        public IEnumerator LoadAsync<T>(string path, ISubscribeHandle<T> subscribe) where T : Object
        {
            yield break;
        }

        public void Unload(Object obj)
        {
            refCount--;
        }

        public static InternalRuntimeBundleHandle Create(RuntimeBundleManifest manifest, AssetBundle bundle)
        {
            InternalRuntimeBundleHandle runtimeAssetBundleHandle = Engine.Class.Loader<InternalRuntimeBundleHandle>();
            runtimeAssetBundleHandle.bundle = bundle;
            runtimeAssetBundleHandle.name = manifest.name;
            runtimeAssetBundleHandle.module = manifest.owner;
            runtimeAssetBundleHandle.refCount = 0;
            runtimeAssetBundleHandle._handles = new HashSet<Object>();
            return runtimeAssetBundleHandle;
        }
    }
}
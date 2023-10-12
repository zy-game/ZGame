using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    internal sealed class AssetBundleRuntimeHandle : IDisposable
    {
        public string name { get; private set; }
        public uint refCount { get; private set; }
        public string module { get; private set; }

        private AssetBundle bundle;
        private List<Object> _handles;

        public void Dispose()
        {
            _handles.Clear();
            bundle?.Unload(true);
            bundle = null;
            name = String.Empty;
            refCount = 0;
            module = String.Empty;
        }

        public bool Contains(Object target)
        {
            return _handles.Contains(target);
        }


        public Object Load(string path)
        {
            if (bundle is null)
            {
                return default;
            }

            Object temp = _handles.Find(x => x.name == Path.GetFileNameWithoutExtension(path));
            if (temp != null)
            {
                refCount++;
                return temp;
            }

            temp = bundle.LoadAsset(path);
            if (temp == null)
            {
                return default;
            }

            _handles.Add(temp);
            refCount++;
            return temp;
        }

        public async UniTask<Object> LoadAsync(string path)
        {
            if (bundle is null)
            {
                return default;
            }

            Object temp = _handles.Find(x => x.name == Path.GetFileNameWithoutExtension(path));
            if (temp != null)
            {
                refCount++;
                return temp;
            }

            temp = await bundle.LoadAssetAsync(path);
            if (temp == null)
            {
                return default;
            }

            refCount++;
            _handles.Add(temp);
            return temp;
        }

        public void Unload(Object obj)
        {
            refCount--;
            _handles.Remove(obj);
        }

        public static AssetBundleRuntimeHandle Create(GameAssetBundleManifest manifest, AssetBundle bundle)
        {
            AssetBundleRuntimeHandle runtimeAssetBundleHandle = Activator.CreateInstance<AssetBundleRuntimeHandle>();
            runtimeAssetBundleHandle.bundle = bundle;
            runtimeAssetBundleHandle.name = manifest.name;
            runtimeAssetBundleHandle.module = manifest.owner;
            runtimeAssetBundleHandle.refCount = 0;
            runtimeAssetBundleHandle._handles = new List<Object>();
            return runtimeAssetBundleHandle;
        }
    }
}
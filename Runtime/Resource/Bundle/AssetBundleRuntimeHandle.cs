using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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


        public T Load<T>(string path) where T : Object
        {
            if (bundle is null)
            {
                return default;
            }

            Object temp = _handles.Find(x => x.name == Path.GetFileNameWithoutExtension(path));
            if (temp != null)
            {
                refCount++;
                return (T)temp;
            }

            temp = bundle.LoadAsset<T>(path);
            _handles.Add(temp);
            refCount++;
            return (T)temp;
        }

        public IEnumerator LoadAsync<T>(string path, ISubscriber<T> subscribe) where T : Object
        {
            if (bundle is null)
            {
                subscribe.Execute(default);
                yield break;
            }

            Object temp = _handles.Find(x => x.name == Path.GetFileNameWithoutExtension(path));
            if (temp != null)
            {
                refCount++;
                subscribe.Execute(temp);
                yield break;
            }

            AssetBundleRequest request = bundle.LoadAssetAsync<T>(path);
            yield return request;
            if (request.isDone is false || request.asset is null)
            {
                subscribe.Execute(default);
                yield break;
            }

            refCount++;
            _handles.Add(request.asset);
            subscribe.Execute(request.asset);
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
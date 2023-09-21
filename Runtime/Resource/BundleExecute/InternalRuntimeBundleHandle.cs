using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    internal sealed class InternalRuntimeBundleHandle : IDisposable
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
            Object temp = default;
#if UNITY_EDITOR
            if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset == Switch.Off)
            {
                temp = _handles.Find(x => x.name == Path.GetFileNameWithoutExtension(path));
                if (temp == null)
                {
                    temp = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                    _handles.Add(temp);
                }

                return (T)temp;
            }
#endif
            if (bundle is null)
            {
                return default;
            }

            temp = bundle.LoadAsset<T>(path);
            _handles.Add(temp);
            return (T)temp;
        }

        public IEnumerator LoadAsync<T>(string path, ISubscribeHandle<T> subscribe) where T : Object
        {
            Object temp = default;
#if UNITY_EDITOR
            if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset == Switch.Off)
            {
                temp = _handles.Find(x => x.name == Path.GetFileNameWithoutExtension(path));
                if (temp == null)
                {
                    temp = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                    _handles.Add(temp);
                }

                subscribe.Execute(temp);
                yield break;
            }
#endif

            if (bundle is null)
            {
                subscribe.Execute(default);
                yield break;
            }

            AssetBundleRequest request = bundle.LoadAssetAsync<T>(path);
            yield return request;
            if (request.isDone is false || request.asset is null)
            {
                subscribe.Execute(default);
                yield break;
            }

            _handles.Add(request.asset);
            subscribe.Execute(request.asset);
        }

        public void Unload(Object obj)
        {
            refCount--;
            _handles.Remove(obj);
        }

        public static InternalRuntimeBundleHandle Create(RuntimeBundleManifest manifest, AssetBundle bundle)
        {
            InternalRuntimeBundleHandle runtimeAssetBundleHandle = Activator.CreateInstance<InternalRuntimeBundleHandle>();
            runtimeAssetBundleHandle.bundle = bundle;
            runtimeAssetBundleHandle.name = manifest.name;
            runtimeAssetBundleHandle.module = manifest.owner;
            runtimeAssetBundleHandle.refCount = 0;
            runtimeAssetBundleHandle._handles = new List<Object>();
            return runtimeAssetBundleHandle;
        }
    }
}
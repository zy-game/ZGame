using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Window;

namespace ZGame.Resource
{
    public class ResourcePackageHandle : IDisposable
    {
        public string name { get; }
        private List<ResHandle> cacheList;
        private AssetBundle bundle { get; }
        public int refCount { get; private set; }

        internal ResourcePackageHandle(string title)
        {
            refCount = 0;
            this.name = title;
            cacheList = new List<ResHandle>();
            Debug.Log("load success :" + title);
        }

        internal ResourcePackageHandle(AssetBundle bundle) : this(bundle.name)
        {
            this.bundle = bundle;
        }

        public void Setup(ResHandle handle)
        {
            cacheList.Add(handle);
        }

        public void AddRef()
        {
            refCount++;
        }

        public void RemoveRef()
        {
            refCount--;
        }

        public bool TryGetValue(string path, out ResHandle handle)
        {
            handle = cacheList.Find(x => x.path == path);
            return handle is not null;
        }

        public bool Contains(ResHandle obj)
        {
            return cacheList.Contains(obj);
        }

        public void Release(ResHandle obj)
        {
            obj.Release();
            if (obj.refCount > 0)
            {
                return;
            }

            obj.Dispose();
            cacheList.Remove(obj);
        }

        public ResHandle LoadAsset(string path)
        {
            if (TryGetValue(path, out ResHandle resHandle))
            {
                return resHandle;
            }

            var asset = bundle.LoadAsset(path);
            if (asset == null)
            {
                return default;
            }

            cacheList.Add(resHandle = new ResHandle(this, asset, path));
            return resHandle;
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path, ILoadingHandle loadingHandle)
        {
            if (TryGetValue(path, out ResHandle resHandle))
            {
                return resHandle;
            }

            var asset = await bundle.LoadAssetAsync(path).ToUniTask(loadingHandle);
            if (asset == null)
            {
                return default;
            }

            cacheList.Add(resHandle = new ResHandle(this, asset, path));
            return resHandle;
        }

        public void Dispose()
        {
            bundle.Unload(true);
            refCount = 0;
            cacheList.ForEach(x => x.Dispose());
            cacheList.Clear();
        }
    }
}
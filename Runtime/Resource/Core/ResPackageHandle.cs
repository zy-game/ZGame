using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Window;

namespace ZGame.Resource
{
    public class ResPackageHandle : IDisposable
    {
        public string name { get; }
        public AssetBundle bundle { get; }
        public int refCount { get; private set; }

        public bool DefaultPackage { get; }

        private List<ResHandle> cacheList;

        internal ResPackageHandle(string title, bool isDefault)
        {
            refCount = 0;
            this.name = title;
            this.DefaultPackage = isDefault;
            cacheList = new List<ResHandle>();
        }

        internal ResPackageHandle(AssetBundle bundle, bool isDefault) : this(bundle.name, isDefault)
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


        public bool Release(string path)
        {
            if (TryGetValue(path, out ResHandle obj) is false)
            {
                return false;
            }

            obj.Release();
            return true;
        }

        public void Dispose()
        {
            Debug.Log("释放资源包:" + name);
            refCount = 0;
            cacheList.ForEach(x => x.Dispose());
            cacheList.Clear();
            bundle?.Unload(true);
            Resources.UnloadUnusedAssets();
        }
    }
}
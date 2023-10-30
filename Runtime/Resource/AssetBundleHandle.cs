using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class AssetBundleHandle : IDisposable
    {
        public string name => bundle?.name;
        private AssetBundle bundle;
        private List<AssetObjectHandle> _results;
        public int refCount { get; private set; }

        internal AssetBundleHandle(AssetBundle bundle)
        {
            this.bundle = bundle;
            _results = new List<AssetObjectHandle>();
            refCount = 0;
        }

        public bool Contains(AssetObjectHandle obj)
        {
            return _results.Find(x => x.Equals(obj)) != null;
        }

        public bool Contains(string path)
        {
            if (bundle == null)
            {
                return false;
            }

            return bundle.Contains(path);
        }

        public void Unload(AssetObjectHandle obj)
        {
            if (Contains(obj) is false)
            {
                return;
            }

            refCount--;
        }

        public AssetObjectHandle Load(string path)
        {
            if (Contains(path) is false)
            {
                return default;
            }

            AssetObjectHandle resObject = _results.Find(x => x.path.Equals(path));
            if (resObject is not null)
            {
                return resObject;
            }

            UnityEngine.Object obj = bundle.LoadAsset(path);
            resObject = new AssetObjectHandle(obj, path);
            _results.Add(resObject);
            return resObject;
        }

        public async UniTask<AssetObjectHandle> LoadAsync(string path)
        {
            if (Contains(path) is false)
            {
                return default;
            }

            AssetObjectHandle resObject = _results.Find(x => x.path.Equals(path));
            if (resObject is not null)
            {
                return resObject;
            }

            UnityEngine.Object result = await bundle.LoadAssetAsync<UnityEngine.Object>(path).ToUniTask();
            resObject = new AssetObjectHandle(result, path);
            _results.Add(resObject);
            return resObject;
        }

        public void Dispose()
        {
            bundle.Unload(true);
            bundle = null;
            refCount = 0;
            _results.ForEach(x => x.Dispose());
            _results.Clear();
        }
    }
}
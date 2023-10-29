using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    class RuntimeAssetBundleHandle : IDisposable
    {
        public string name => bundle?.name;
        private AssetBundle bundle;
        private List<ResObject> _results;
        public int refCount { get; private set; }

        public RuntimeAssetBundleHandle(AssetBundle bundle)
        {
            this.bundle = bundle;
            _results = new List<ResObject>();
            refCount = 0;
        }

        public bool Contains(UnityEngine.Object obj)
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

        public void Unload(UnityEngine.Object obj)
        {
            if (Contains(obj) is false)
            {
                return;
            }

            refCount--;
        }

        public ResObject Load(string path)
        {
            if (Contains(path) is false)
            {
                return default;
            }

            ResObject resObject = _results.Find(x => x.path.Equals(path));
            if (resObject is not null)
            {
                return resObject;
            }

            UnityEngine.Object obj = bundle.LoadAsset(path);
            resObject = new ResObject(obj, path);
            _results.Add(resObject);
            return resObject;
        }

        public async UniTask<ResObject> LoadAsync(string path)
        {
            if (Contains(path) is false)
            {
                return default;
            }

            ResObject resObject = _results.Find(x => x.path.Equals(path));
            if (resObject is not null)
            {
                return resObject;
            }

            UnityEngine.Object result = await bundle.LoadAssetAsync<UnityEngine.Object>(path).ToUniTask();
            resObject = new ResObject(result, path);
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
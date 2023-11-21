using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class ABHandle : IDisposable
    {
        public string name => bundle?.name;
        private AssetBundle bundle;
        private List<ResHandle> _results;
        public int refCount { get; private set; }

        internal ABHandle(string title)
        {
        }
        

        internal ABHandle(AssetBundle bundle)
        {
            this.bundle = bundle;
            _results = new List<ResHandle>();
            refCount = 0;
        }

        public void AddRef()
        {
            refCount++;
        }

        public void RemoveRef()
        {
            refCount--;
        }

        public bool Contains(ResHandle obj)
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

        public void Release(ResHandle obj)
        {
            if (obj.refCount > 0)
            {
                return;
            }

            obj.Dispose();
            _results.Remove(obj);
            refCount--;
        }

        public ResHandle Load(string path)
        {
            if (Contains(path) is false)
            {
                return default;
            }

            ResHandle resObject = _results.Find(x => x.path.Equals(path));
            if (resObject is not null)
            {
                return resObject;
            }

            UnityEngine.Object obj = bundle.LoadAsset(path);
            resObject = new ResHandle(this, obj, path);
            _results.Add(resObject);
            return resObject;
        }

        public async UniTask<ResHandle> LoadAsync(string path)
        {
            if (Contains(path) is false)
            {
                return default;
            }

            ResHandle resObject = _results.Find(x => x.path.Equals(path));
            if (resObject is not null)
            {
                return resObject;
            }

            UnityEngine.Object result = await bundle.LoadAssetAsync<UnityEngine.Object>(path).ToUniTask();
            resObject = new ResHandle(this, result, path);
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
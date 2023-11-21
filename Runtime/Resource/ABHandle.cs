using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class Pooled<T> : IDisposable
    {
        private Queue<T> pool;

        public Pooled()
        {
            pool = new Queue<T>();
        }


        public void Dispose()
        {
        }
    }

    public class ABHandle : IDisposable
    {
        public string name { get; }
        private List<ResHandle> resList;
        public AssetBundle bundle { get; }
        public int refCount { get; private set; }

        internal ABHandle(string title)
        {
            refCount = 0;
            this.name = title;
            resList = new List<ResHandle>();
        }

        internal ABHandle(AssetBundle bundle) : this(bundle.name)
        {
            this.bundle = bundle;
        }

        public void AddRef()
        {
            refCount++;
        }

        public void RemoveRef()
        {
            refCount--;
        }

        public void AddRes(ResHandle res)
        {
            resList.Add(res);
        }

        public ResHandle GetRes(string path)
        {
            return resList.Find(x => x.path.Equals(path));
        }

        public bool Contains(ResHandle obj)
        {
            return GetRes(obj.path) != null;
        }

        public bool Contains(string path)
        {
            if (bundle == null)
            {
                return resList.Find(x => x.path.Equals(path)) != null;
            }

            return bundle.Contains(path);
        }

        public void Release(ResHandle obj)
        {
            obj.Release();
            if (obj.refCount > 0)
            {
                return;
            }

            obj.Dispose();
            resList.Remove(obj);
        }


        public void Dispose()
        {
            bundle.Unload(true);
            refCount = 0;
            resList.ForEach(x => x.Dispose());
            resList.Clear();
        }
    }
}
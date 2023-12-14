using System;
using UnityEngine;

namespace ZGame.Resource
{
    public sealed class ResHandle : IDisposable
    {
        private int count;
        private object obj;
        private ResourcePackageHandle parent;
        public int refCount => count;
        public object asset => obj;
        public string path { get; private set; }

        public ResHandle(ResourcePackageHandle parent, object obj, string path)
        {
            this.obj = obj;
            this.path = path;
            this.parent = parent;
        }

        public T Require<T>()
        {
            count++;
            this.parent.AddRef();
            return obj == null ? default(T) : (T)obj;
        }

        public void Release()
        {
            count--;
            this.parent.RemoveRef();
        }

        public override bool Equals(object target)
        {
            if (target is UnityEngine.Object o)
            {
                return obj.Equals(o);
            }

            return base.Equals(target);
        }

        public void Dispose()
        {
            obj = null;
            parent = null;
            count = 0;
            path = String.Empty;
        }
    }
}
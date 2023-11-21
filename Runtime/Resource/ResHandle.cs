using System;
using UnityEngine;

namespace ZGame.Resource
{
    public sealed class ResHandle : IDisposable
    {
        private UnityEngine.Object obj;
        private ABHandle parent;
        public int refCount => count;
        public string path { get; }
        private int count;

        public ResHandle(ABHandle parent, UnityEngine.Object obj, string path)
        {
            this.obj = obj;
            this.path = path;
            this.parent = parent;
        }

        public T Require<T>() where T : UnityEngine.Object
        {
            count++;
            this.parent.AddRef();
            return (T)obj;
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
            Resources.UnloadUnusedAssets();
            obj = null;
        }
    }
}
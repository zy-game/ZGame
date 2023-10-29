using System;
using UnityEngine;

namespace ZGame.Resource
{
    public sealed class ResObject : IDisposable
    {
        private UnityEngine.Object obj;
        public int refCount => count;
        public string path { get; }
        private int count;

        public ResObject(UnityEngine.Object obj, string path)
        {
            this.obj = obj;
            this.path = path;
        }

        public T Require<T>() where T : UnityEngine.Object
        {
            count++;
            return (T)obj;
        }

        public void Release()
        {
            count--;
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
            Resources.UnloadAsset(obj);
            obj = null;
        }
    }
}
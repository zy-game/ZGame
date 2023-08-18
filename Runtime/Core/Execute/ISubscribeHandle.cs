using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    public interface ISubscribeHandle : IReference
    {
        void Execute(object args);
        void Merge(ISubscribeHandle subscribe);
        void Unmerge(ISubscribeHandle subscribe);

        public static ISubscribeHandle Create(Action action)
        {
            return InternalGameSubscribeHandle<object>.Create(args => action(), action);
        }

        public static ISubscribeHandle<T> Create<T>(Action<T> callback)
        {
            return InternalGameSubscribeHandle<T>.Create(callback, callback);
        }
    }

    public interface ISubscribeHandle<T> : ISubscribeHandle
    {
        void Execute(T args);

        void Merge(ISubscribeHandle<T> subscribe)
        {
            Merge((ISubscribeHandle)subscribe);
        }

        void Unmerge(ISubscribeHandle<T> subscribe)
        {
            Unmerge((ISubscribeHandle)subscribe);
        }
    }

    class InternalGameSubscribeHandle<T> : ISubscribeHandle<T>
    {
        protected Action<T> method;
        private object handle;

        public virtual void Execute(object args)
        {
            Execute((T)args);
        }

        public void Execute(T args)
        {
            method?.Invoke(args);
            if (args is IReference reference)
            {
                Engine.Class.Release(reference);
            }
        }

        public void Merge(ISubscribeHandle subscribe)
        {
            this.method += ((InternalGameSubscribeHandle<T>)subscribe).method;
        }

        public void Unmerge(ISubscribeHandle subscribe)
        {
            this.method -= ((InternalGameSubscribeHandle<T>)subscribe).method;
        }

        public void Release()
        {
            method = null;
            handle = null;
            GC.SuppressFinalize(this);
        }


        internal static InternalGameSubscribeHandle<T> Create(Action<T> callback, object handle)
        {
            InternalGameSubscribeHandle<T> internalGameSubscribeHandle = Engine.Class.Loader<InternalGameSubscribeHandle<T>>();
            internalGameSubscribeHandle.method = callback;
            internalGameSubscribeHandle.handle = handle;
            return internalGameSubscribeHandle;
        }
    }
}
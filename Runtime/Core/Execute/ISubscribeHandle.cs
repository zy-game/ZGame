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
            return ISubscribeHandle<object>.InternalGameSubscribeHandle.Create(args => action());
        }

        public static ISubscribeHandle<T> Create<T>(Action<T> callback)
        {
            return ISubscribeHandle<T>.InternalGameSubscribeHandle.Create(callback);
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

        class InternalGameSubscribeHandle : ISubscribeHandle<T>
        {
            protected Action<T> method;

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
                this.method += ((InternalGameSubscribeHandle)subscribe).method;
            }

            public void Unmerge(ISubscribeHandle subscribe)
            {
                this.method -= ((InternalGameSubscribeHandle)subscribe).method;
            }

            public void Release()
            {
                method = null;
                GC.SuppressFinalize(this);
            }


            internal static InternalGameSubscribeHandle Create(Action<T> callback)
            {
                InternalGameSubscribeHandle internalGameSubscribeHandle = Engine.Class.Loader<InternalGameSubscribeHandle>();
                internalGameSubscribeHandle.method = callback;
                return internalGameSubscribeHandle;
            }
        }
    }
}
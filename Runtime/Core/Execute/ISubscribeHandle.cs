using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    public interface ISubscribeHandle : IReference
    {
        void Execute(object value);
        void Execute(Exception exception);

        public static ISubscribeHandle Create(Action action)
        {
            return ISubscribeHandle<object>.DefaultMethodSubscribeHandle.Create(args => action(), action);
        }

        public static ISubscribeHandle<T> Create<T>(Action<T> callback)
        {
            return ISubscribeHandle<T>.DefaultMethodSubscribeHandle.Create(callback, callback);
        }
    }

    public interface ISubscribeHandle<T> : ISubscribeHandle
    {
        class DefaultMethodSubscribeHandle : ISubscribeHandle<T>
        {
            private bool isComplete;
            private Action<T> method;
            private Exception exception;
            private object handle;

            public void Execute(object value)
            {
                method((T)value);
            }

            public void Execute(Exception exception)
            {
                this.exception = exception;
                Engine.Console.Error(this.exception);
                isComplete = true;
            }

            public override bool Equals(object obj)
            {
                if (obj is DefaultMethodSubscribeHandle temp)
                {
                    return handle.Equals(temp.handle);
                }

                return method.Equals(obj);
            }

            public void Release()
            {
                isComplete = false;
                exception = null;
                method = null;
                handle = null;
                GC.SuppressFinalize(this);
            }


            public static DefaultMethodSubscribeHandle Create(Action<T> callback, object handle)
            {
                DefaultMethodSubscribeHandle defaultMethodSubscribeHandle = Engine.Class.Loader<DefaultMethodSubscribeHandle>();
                defaultMethodSubscribeHandle.method = callback;
                defaultMethodSubscribeHandle.handle = handle;
                defaultMethodSubscribeHandle.isComplete = false;
                return defaultMethodSubscribeHandle;
            }
        }
    }
}
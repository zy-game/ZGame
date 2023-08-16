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
            return DefaultMethodSubscribeHandle<object>.Create(args => action());
        }
    }

    public interface ISubscribeHandle<T> : ISubscribeHandle
    {
        public static ISubscribeHandle<T> Create(Action<T> callback)
        {
            return DefaultMethodSubscribeHandle<T>.Create(callback);
        }
    }

    class DefaultMethodSubscribeHandle<T> : ISubscribeHandle<T>
    {
        private float time;
        private bool isComplete;
        private Action<T> method;
        private Exception exception;

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

        public IEnumerator ExecuteComplete(float timeout = 0)
        {
            time = timeout == 0 ? float.MaxValue : Time.realtimeSinceStartup + timeout;
            yield return WaitFor.Create(CheckCompletion);
        }

        private bool CheckCompletion()
        {
            return isComplete || Time.realtimeSinceStartup > time;
        }

        public override bool Equals(object obj)
        {
            return method.Equals(obj);
        }

        public void Release()
        {
            method = null;
            time = 0;
            isComplete = false;
            exception = null;
            GC.SuppressFinalize(this);
        }

        public static DefaultMethodSubscribeHandle<T> Create(Action<T> callback)
        {
            DefaultMethodSubscribeHandle<T> defaultMethodSubscribeHandle = Engine.Class.Loader<DefaultMethodSubscribeHandle<T>>();
            defaultMethodSubscribeHandle.method = callback;
            defaultMethodSubscribeHandle.isComplete = false;
            return defaultMethodSubscribeHandle;
        }
    }
}
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
        IEnumerator ExecuteComplete(float timeout = 0);

        public static ISubscribeHandle Create(Action action)
        {
            return DefaultMethodSubscribeHandle<object>.Create(args => action());
        }

        public static ISubscribeHandle<T> Create<T>(Action<T> callback)
        {
            return DefaultMethodSubscribeHandle<T>.Create(callback);
        }
    }

    public interface ISubscribeHandle<T> : ISubscribeHandle
    {
        void Execute(T value);
    }

    class DefaultMethodSubscribeHandle<T> : ISubscribeHandle<T>
    {
        private float time;
        private bool isComplete;
        private Action<T> method;
        private Exception exception;

        public void Execute(object value)
        {
            Execute((T)value);
        }

        public void Execute(T value)
        {
            method?.Invoke((T)value);
            isComplete = true;
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
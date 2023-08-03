using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    public interface ISubscribeExecuteHandle : IReference
    {
        object result { get; }
        void Execute(object value);
        void Execute(Exception exception);
        IEnumerator Wait(float timeout = 0);

        public static ISubscribeExecuteHandle Create(Action action)
        {
            return DefaultMethodSubscribeExecuteHandle.Create(action);
        }
    }

    public interface ISubscribeExecuteHandle<T> : ISubscribeExecuteHandle
    {
        T result { get; }
        void Execute(T value);
        void Execute(Exception exception);
        IEnumerator Wait(float timeout = 0);

        public static ISubscribeExecuteHandle<T> Create()
        {
            return Engine.Class.Loader<DefaultSubscribeExecuteHandle<T>>();
        }

        public static ISubscribeExecuteHandle<T> Create(Action<T> callback)
        {
            return DefaultMethodSubscribeExecuteHandle<T>.Create(callback);
        }
    }

    class DefaultMethodSubscribeExecuteHandle : ISubscribeExecuteHandle
    {
        private float time;
        private Action method;
        private Exception exception;
        public object result { get; private set; }

        public void Release()
        {
            method = null;
            result = null;
        }

        public void Execute(object value)
        {
            result = value;
            method?.Invoke();
        }

        public void Execute(Exception exception)
        {
            Engine.Console.Error(exception);
            this.exception = exception;
            method?.Invoke();
        }

        public IEnumerator Wait(float timeout = 0)
        {
            time = timeout == 0 ? float.MaxValue : Time.realtimeSinceStartup + timeout;
            yield return new WaitUntil(CheckCompletion);
        }

        public override bool Equals(object obj)
        {
            return method.Equals(obj);
        }

        private bool CheckCompletion()
        {
            return result is not null || exception is not null || Time.realtimeSinceStartup > time;
        }

        public static DefaultMethodSubscribeExecuteHandle Create(Action callback)
        {
            DefaultMethodSubscribeExecuteHandle defaultMethodSubscribeExecuteHandle = Engine.Class.Loader<DefaultMethodSubscribeExecuteHandle>();
            defaultMethodSubscribeExecuteHandle.method = callback;
            return defaultMethodSubscribeExecuteHandle;
        }
    }

    class DefaultMethodSubscribeExecuteHandle<T> : ISubscribeExecuteHandle<T>
    {
        private Action<T> method;

        private float time;
        private Exception exception;
        public T result { get; private set; }

        object ISubscribeExecuteHandle.result { get; }

        public void Execute(object value)
        {
            Execute((T)value);
        }

        public void Execute(T value)
        {
            result = value;
            method?.Invoke((T)value);
        }

        public void Execute(Exception exception)
        {
            this.exception = exception;
        }

        public IEnumerator Wait(float timeout = 0)
        {
            time = timeout == 0 ? float.MaxValue : Time.realtimeSinceStartup + timeout;
            yield return new WaitUntil(CheckCompletion);
        }

        private bool CheckCompletion()
        {
            return result is not null || exception is not null || Time.realtimeSinceStartup > time;
        }

        public override bool Equals(object obj)
        {
            return method.Equals(obj);
        }

        public static DefaultMethodSubscribeExecuteHandle<T> Create(Action<T> callback)
        {
            DefaultMethodSubscribeExecuteHandle<T> defaultMethodSubscribeExecuteHandle = Engine.Class.Loader<DefaultMethodSubscribeExecuteHandle<T>>();
            defaultMethodSubscribeExecuteHandle.method = callback;
            return defaultMethodSubscribeExecuteHandle;
        }

        public void Release()
        {
            method = null;
        }
    }

    class DefaultSubscribeExecuteHandle<T> : ISubscribeExecuteHandle<T>
    {
        private float time;
        private Exception exception;
        public T result { get; private set; }


        public void Execute(T value)
        {
            result = value;
        }

        object ISubscribeExecuteHandle.result { get; }

        public void Execute(object value)
        {
            result = (T)value;
        }

        public void Execute(Exception exception)
        {
            this.exception = exception;
        }

        public IEnumerator Wait(float timeout = 0)
        {
            time = timeout == 0 ? float.MaxValue : Time.realtimeSinceStartup + timeout;
            yield return new WaitUntil(CheckCompletion);
        }

        private bool CheckCompletion()
        {
            return result is not null || exception is not null || Time.realtimeSinceStartup > time;
        }

        public void Release()
        {
            result = default;
        }
    }
}
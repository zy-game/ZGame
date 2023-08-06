using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    public interface ISubscribeExecuteHandle : IReference
    {
        void Execute(object value);
        void Execute(Exception exception);
        IEnumerator ExecuteComplete(float timeout = 0);

        public static ISubscribeExecuteHandle Create(Action action)
        {
            return DefaultMethodSubscribeExecuteHandle.Create(action);
        }
    }

    public interface ISubscribeExecuteHandle<T> : ISubscribeExecuteHandle
    {
        void Execute(T value);

        public static ISubscribeExecuteHandle<T> Create(Action<T> callback)
        {
            return DefaultMethodSubscribeExecuteHandle<T>.Create(callback);
        }
    }

    class DefaultMethodSubscribeExecuteHandle : ISubscribeExecuteHandle
    {
        private float time;
        private Action method;
        private bool isComplete;
        private Exception exception;


        public void Release()
        {
            method = null;
        }

        public void Execute(object value)
        {
            method?.Invoke();
            isComplete = true;
        }

        public void Execute(Exception exception)
        {
            this.exception = exception;
            Engine.Console.Error(exception);
            isComplete = true;
        }

        public IEnumerator ExecuteComplete(float timeout = 0)
        {
            time = timeout == 0 ? float.MaxValue : Time.realtimeSinceStartup + timeout;
            yield return WaitFor.Create(CheckCompletion);
        }

        public override bool Equals(object obj)
        {
            return method.Equals(obj);
        }

        private bool CheckCompletion()
        {
            return isComplete || Time.realtimeSinceStartup > time;
        }

        public static DefaultMethodSubscribeExecuteHandle Create(Action callback)
        {
            DefaultMethodSubscribeExecuteHandle defaultMethodSubscribeExecuteHandle = Engine.Class.Loader<DefaultMethodSubscribeExecuteHandle>();
            defaultMethodSubscribeExecuteHandle.method = callback;
            defaultMethodSubscribeExecuteHandle.isComplete = false;
            return defaultMethodSubscribeExecuteHandle;
        }
    }

    class DefaultMethodSubscribeExecuteHandle<T> : ISubscribeExecuteHandle<T>
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

        public static DefaultMethodSubscribeExecuteHandle<T> Create(Action<T> callback)
        {
            DefaultMethodSubscribeExecuteHandle<T> defaultMethodSubscribeExecuteHandle = Engine.Class.Loader<DefaultMethodSubscribeExecuteHandle<T>>();
            defaultMethodSubscribeExecuteHandle.method = callback;
            defaultMethodSubscribeExecuteHandle.isComplete = false;
            return defaultMethodSubscribeExecuteHandle;
        }
    }
}
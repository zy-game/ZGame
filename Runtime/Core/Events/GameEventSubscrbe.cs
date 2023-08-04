using System;
using System.Collections;
using UnityEngine;

namespace ZEngine
{
    public class GameEventSubscrbe<T> : ISubscribeExecuteHandle<T> where T : GameEventArgs<T>
    {
        private Action<T> method;
        public T result { get; private set; }
        public Exception exception { get; private set; }

        public GameEventSubscrbe()
        {
        }

        public virtual void Release()
        {
            result = null;
        }

        public void Execute(T value)
        {
            result = value;
            OnTrigger(value);
        }


        public void Execute(object value)
        {
            result = (T)value;
        }

        public void Execute(Exception exception)
        {
            this.exception = exception;
        }

        public IEnumerator ExecuteComplete(float timeout = 0)
        {
            throw new NotImplementedException();
        }

        public virtual void OnTrigger(T evtArgs)
        {
            method?.Invoke(evtArgs);
        }

        public static GameEventSubscrbe<T> Create(Action<T> callback)
        {
            GameEventSubscrbe<T> gameEventSubscrbe = Engine.Class.Loader<GameEventSubscrbe<T>>();
            gameEventSubscrbe.method = callback;
            return gameEventSubscrbe;
        }
    }
}
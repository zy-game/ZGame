using System;
using System.Collections;
using UnityEngine;

namespace ZEngine
{
    public class GameEventSubscrbe<T> : ISubscribeHandle where T : GameEventArgs<T>
    {
        private Action<T> method;
        public T result { get; private set; }
        public Exception exception { get; private set; }
        public GameEventType type { get; private set; }

        public virtual void Release()
        {
            result = null;
            method = null;
            exception = null;
            GC.SuppressFinalize(this);
        }

        public void Execute(T value)
        {
            result = value;
            OnTrigger(value);
        }


        public void Execute(object value)
        {
            result = (T)value;
            OnTrigger(result);
        }

        public void Execute(Exception exception)
        {
            this.exception = exception;
            OnTrigger(result);
        }

        public IEnumerator ExecuteComplete(float timeout = 0)
        {
            throw new NotImplementedException();
        }

        public virtual void OnTrigger(T evtArgs)
        {
            method?.Invoke(evtArgs);
        }

        public override bool Equals(object obj)
        {
            return method.Equals(obj);
        }

        public static GameEventSubscrbe<T> Create(Action<T> callback)
        {
            GameEventSubscrbe<T> gameEventSubscrbe = Engine.Class.Loader<GameEventSubscrbe<T>>();
            gameEventSubscrbe.method = callback;
            return gameEventSubscrbe;
        }

        public static GameEventSubscrbe<T> Create(GameEventType type, Action<T> callback)
        {
            GameEventSubscrbe<T> gameEventSubscrbe = Create(callback);
            gameEventSubscrbe.type = type;
            return gameEventSubscrbe;
        }

        public static GameEventSubscrbe<T> operator +(GameEventSubscrbe<T> l, Action<T> callback)
        {
            l.method += callback;
            return l;
        }

        public static GameEventSubscrbe<T> operator -(GameEventSubscrbe<T> l, Action<T> callback)
        {
            l.method -= callback;
            return l;
        }

        public static GameEventSubscrbe<T> operator +(GameEventSubscrbe<T> l, GameEventSubscrbe<T> callback)
        {
            l.method += callback.method;
            return l;
        }

        public static GameEventSubscrbe<T> operator -(GameEventSubscrbe<T> l, GameEventSubscrbe<T> callback)
        {
            l.method -= callback.method;
            return l;
        }
    }
}
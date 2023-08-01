using System;

namespace ZEngine
{
    public class SubscribeMethodHandle<T> : ISubscribe<T>
    {
        private Action<T> callback;


        public void Execute(T args)
        {
            callback?.Invoke(args);
        }

        public void Release()
        {
            callback = null;
        }

        public void Execute(params object[] args)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is SubscribeMethodHandle<T> target)
            {
                return target.callback == callback;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return callback.GetHashCode();
        }

        public static SubscribeMethodHandle<T> Create(Action<T> action)
        {
            SubscribeMethodHandle<T> subscribeMethod = Engine.Class.Loader<SubscribeMethodHandle<T>>();
            subscribeMethod.callback = action;
            return subscribeMethod;
        }
    }
}
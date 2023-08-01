using System;

namespace ZEngine
{
    public sealed class SubscribeMethodHandle : ISubscribe
    {
        private Action callback;


        public void Release()
        {
            callback = null;
        }

        public void Execute(params object[] args)
        {
            callback?.Invoke();
        }

        public override bool Equals(object obj)
        {
            if (obj is SubscribeMethodHandle target)
            {
                return target.callback == callback;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return callback.GetHashCode();
        }

        public static SubscribeMethodHandle Create(Action action)
        {
            SubscribeMethodHandle subscribeMethod = Engine.Class.Loader<SubscribeMethodHandle>();
            subscribeMethod.callback = action;
            return subscribeMethod;
        }
    }

   
}
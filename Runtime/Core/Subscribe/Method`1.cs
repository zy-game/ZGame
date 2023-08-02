using System;

namespace ZEngine
{
    public class Method<T> : ISubscribe<T>
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
            if (obj is Method<T> target)
            {
                return target.callback == callback;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return callback.GetHashCode();
        }

        public static Method<T> Create(Action<T> action)
        {
            Method<T> methodSubscribe = Engine.Class.Loader<Method<T>>();
            methodSubscribe.callback = action;
            return methodSubscribe;
        }
    }
}
using System;

namespace ZEngine
{
    public sealed class Method : ISubscribe
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
            if (obj is Method target)
            {
                return target.callback == callback;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return callback.GetHashCode();
        }

        public static Method Create(Action action)
        {
            Method methodSubscribe = Engine.Class.Loader<Method>();
            methodSubscribe.callback = action;
            return methodSubscribe;
        }
    }
}
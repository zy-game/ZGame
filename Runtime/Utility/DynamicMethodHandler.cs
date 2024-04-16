using System;

namespace ZGame.Networking
{
    class DynamicMethodHandler : IReference
    {
        private Action<object> callback;
        private object target;

        public void Invoke(object msg)
        {
            this.callback?.Invoke(msg);
        }

        public void Release()
        {
            callback = null;
        }

        public override bool Equals(object obj)
        {
            if (obj is DynamicMethodHandler dynamicCallback)
            {
                return dynamicCallback.target == target;
            }

            return target.Equals(obj);
        }

        public static DynamicMethodHandler Create<T>(Action<T> callback)
        {
            DynamicMethodHandler dynamic = RefPooled.Spawner<DynamicMethodHandler>();
            dynamic.callback = x => callback((T)x);
            dynamic.target = callback;
            return dynamic;
        }
    }
}
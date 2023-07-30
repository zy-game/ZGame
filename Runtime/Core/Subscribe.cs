using System;

namespace ZEngine
{
    
    public struct Subscribe : ISubscribe
    {
        private Action callback;

        public Subscribe(Action callback)
        {
            this.callback = callback;
        }

        public void Release()
        {
            callback = null;
        }

        public void Execute(params object[] args)
        {
            callback?.Invoke();
        }

        public static Subscribe Create(Action action)
        {
            return new Subscribe(action);
        }

        public static implicit operator Subscribe(Action action)
        {
            Subscribe internalSubscribe = new Subscribe(action);
            return internalSubscribe;
        }

        public static explicit operator Action(Subscribe subscribe)
        {
            return subscribe.callback;
        }

        public static bool operator ==(Subscribe subscribe, Action action)
        {
            return subscribe.callback.Equals(action);
        }

        public static bool operator !=(Subscribe subscribe, Action action)
        {
            return subscribe.callback.Equals(action) == false;
        }
    }
}
using System;

namespace ZGame
{
    class CommonCommandHandle : ICommandExecuter
    {
        private Action callback2;
        private Action<object[]> callback;

        public void Dispose()
        {
            callback = null;
        }

        public void Awake()
        {
        }

        public void Executer(params object[] args)
        {
            if (callback is not null)
            {
                callback(args);
            }

            if (callback2 is not null)
            {
                callback2();
            }
        }

        public void Add(Action<object[]> callback)
        {
            this.callback += callback;
        }

        public void Remove(Action<object[]> callback)
        {
            this.callback -= callback;
        }

        public void Add(Action callback)
        {
            this.callback2 += callback;
        }

        public void Remove(Action callback)
        {
            this.callback2 -= callback;
        }
    }
}
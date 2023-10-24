using System;

namespace ZEngine
{
    public interface IProgressHandle : IDisposable
    {
        IProgressHandle SetInfo(string text);
        IProgressHandle SetProgress(float progress);

        public static IProgressHandle Empty => new InternalProgressHandle(_ => { });

        public static IProgressHandle Create(Action<float> action)
        {
            return new InternalProgressHandle(action);
        }

        class InternalProgressHandle : IProgressHandle
        {
            private Action<float> callback;

            public InternalProgressHandle(Action<float> action)
            {
                this.callback = action;
            }

            public void Dispose()
            {
                this.callback = null;
            }

            public IProgressHandle SetInfo(string text)
            {
                return this;
            }

            public IProgressHandle SetProgress(float progress)
            {
                this.callback?.Invoke(progress);
                return this;
            }
        }
    }
}
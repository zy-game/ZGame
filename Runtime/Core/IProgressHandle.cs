using System;

namespace ZEngine
{
    public interface IProgressHandle : IDisposable
    {
        void SetTextInfo(string text);
        void SetProgress(float progress);

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

            public void SetTextInfo(string text)
            {
            }

            public void SetProgress(float progress)
            {
                this.callback?.Invoke(progress);
            }
        }
    }
}
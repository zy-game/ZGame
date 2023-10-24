using System;

namespace ZGame
{
    /// <summary>
    /// 事件管道
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEvent<T> : IEvent
    {
        void Invoke(T args);

        class EventPipeline : IEvent<T>
        {
            public string guid { get; }
            private Action<T> action;
            private bool enable;
            private object owner;


            public EventPipeline(Action<T> action, object owner)
            {
                this.owner = owner;
                this.enable = true;
                this.action = action;
            }

            public void Active()
            {
                this.enable = true;
            }

            public void Inactive()
            {
                this.enable = false;
            }

            public void Dispose()
            {
                this.owner = null;
                this.enable = false;
                this.action = null;
            }

            public void Invoke()
            {
                this.Invoke(default);
            }

            public void Invoke(T args)
            {
                if (this.enable is false)
                {
                    return;
                }

                this.action?.Invoke(args);
            }

            public override bool Equals(object obj)
            {
                return this.owner.Equals(obj);
            }
        }
    }
}
using System;

namespace ZEngine
{
    public interface IBindPipeline : IDisposable
    {
        string name { get; }
        bool actived { get; }
        void Active();
        void Inactive();
    }

    public interface IEventBindPipeline<T> : IBindPipeline
    {
        void AddListener(Action<T> callback);
        void RemoveListener(Action<T> callback);
        void Invoke(T args);

        public static IEventBindPipeline<T> Create(string title, Action<T> callback)
        {
            return new EventBindPipelineHandle(title);
        }

        class EventBindPipelineHandle : IEventBindPipeline<T>
        {
            public string name { get; set; }
            public bool actived { get; set; }

            private event Action<T> Callback;

            public EventBindPipelineHandle(string name)
            {
                this.name = name;
            }

            public void Active()
            {
                this.actived = true;
            }

            public void Inactive()
            {
                this.actived = false;
            }

            public void AddListener(Action<T> callback)
            {
                this.Callback += callback;
            }

            public void RemoveListener(Action<T> callback)
            {
                this.Callback -= callback;
            }

            public void Invoke(T args)
            {
                if (actived is false)
                {
                    return;
                }

                this.Callback?.Invoke(args);
            }

            public void Dispose()
            {
                Callback = null;
                actived = false;
                name = String.Empty;
            }
        }
    }

    public interface IValueBindPipeline<T> : IEventBindPipeline<T>
    {
        T value { get; }
        void SetValue(T value);
        void SetValueWithoutNotify(T value);
    }
}
using System;

namespace ZEngine
{
    public interface IScheduleToken<T> : IToken
    {
        T result { get; }
        bool isComplate { get; }
        void Complate(T result);

        public static IScheduleToken<T> Create()
        {
            return Activator.CreateInstance<InternalScheduleToken>();
        }

        class InternalScheduleToken : IScheduleToken<T>
        {
            public T result { get; set; }
            object IToken.result => result;
            public bool isComplate { get; set; }

            public void Complate(object value)
            {
                Complate((T)value);
            }

            public void Complate(T result)
            {
                isComplate = true;
                this.result = result;
            }

            public void Dispose()
            {
                result = default;
                isComplate = false;
                GC.SuppressFinalize(this);
            }
        }
    }
}
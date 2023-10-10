using System;

namespace ZEngine
{
    public interface IScheduleHandleToken : IDisposable
    {
        object result { get; }
        bool isComplate { get; }
        void Complate(object value);

        public static IScheduleHandleToken Create()
        {
            return IScheduleHandleToken<object>.InternalScheduleToken.Create();
        }

        public static IScheduleHandleToken<T> Create<T>()
            => IScheduleHandleToken<T>.InternalScheduleToken.Create();
    }

    public interface IScheduleHandleToken<T> : IScheduleHandleToken
    {
        T result { get; }

        class InternalScheduleToken : IScheduleHandleToken<T>
        {
            object IScheduleHandleToken.result => result;
            public T result { get; set; }
            public bool isComplate { get; set; }

            public void Complate(object value)
            {
                isComplate = true;
                this.result = (T)value;
            }

            public void Dispose()
            {
                result = default;
                isComplate = false;
                GC.SuppressFinalize(this);
            }

            public static InternalScheduleToken Create()
            {
                return Activator.CreateInstance<InternalScheduleToken>();
            }
        }
    }
}
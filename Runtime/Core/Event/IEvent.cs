using System;

namespace ZGame
{
    /// <summary>
    /// 事件管道
    /// </summary>
    public interface IEvent : IEntity
    {
        /// <summary>
        /// 激活管道
        /// </summary>
        void Active();

        /// <summary>
        /// 禁用管道
        /// </summary>
        void Inactive();

        /// <summary>
        /// 调用事件
        /// </summary>
        void Invoke();

        public static IEvent Builder(Action action)
        {
            return new IEvent<object>.EventPipeline(_ => action(), action);
        }

        public static IEvent<T> Builder<T>(Action<T> action)
        {
            return new IEvent<T>.EventPipeline(action, action);
        }

        public static IEvent Empty => Builder(() => { });
    }
}
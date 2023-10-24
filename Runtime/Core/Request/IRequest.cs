namespace ZGame
{
    /// <summary>
    /// 请求返回数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequest<T> : IEntity
    {
        /// <summary>
        /// 返回结果
        /// </summary>
        T result { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        IError error { get; }

        /// <summary>
        /// 确定请求是否成功
        /// </summary>
        bool EnsureRequestSuccessfuly();
    }
}
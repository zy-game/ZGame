namespace ZEngine
{
    /// <summary>
    /// 执行器状态
    /// </summary>
    public enum Status : byte
    {
        None,

        /// <summary>
        /// 正在执行
        /// </summary>
        Execute,

        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 失败
        /// </summary>
        Failed,

        /// <summary>
        /// 取消
        /// </summary>
        Cancel,
    }
}
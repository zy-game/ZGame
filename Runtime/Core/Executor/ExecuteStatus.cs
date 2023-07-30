namespace ZEngine
{
    /// <summary>
    /// 执行器状态
    /// </summary>
    public enum ExecuteStatus : byte
    {
        None,

        /// <summary>
        /// 正在执行
        /// </summary>
        Execute,

        /// <summary>
        /// 暂停中
        /// </summary>
        Paused,

        /// <summary>
        /// 执行成功
        /// </summary>
        Success,

        /// <summary>
        /// 执行失败
        /// </summary>
        Failed,

        /// <summary>
        /// 已取消
        /// </summary>
        Canceled
    }
}
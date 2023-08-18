namespace ZEngine
{
    /// <summary>
    /// 开关状态
    /// </summary>
    public enum Switch : byte
    {
        /// <summary>
        /// 关闭
        /// </summary>
        Off,

        /// <summary>
        /// 开启
        /// </summary>
        On,
    }

    /// <summary>
    /// 执行器状态
    /// </summary>
    public enum Status : byte
    {
        /// <summary>
        /// 无状态
        /// </summary>
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
    }
}
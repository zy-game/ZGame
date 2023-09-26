namespace ZEngine.Game
{
    /// <summary>
    /// 游戏同步组件
    /// </summary>
    public interface ISynchroComponent : INetworkComponent
    {
        /// <summary>
        /// 当前帧编号
        /// </summary>
        ulong currentFreameCount { get; }

        /// <summary>
        /// 当前服务器帧编号
        /// </summary>
        ulong currentServerFrameCount { get; }
        
        
    }
}
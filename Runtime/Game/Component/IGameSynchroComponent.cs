namespace ZEngine.Game
{
    /// <summary>
    /// 游戏同步组件
    /// </summary>
    public interface IGameSynchroComponent : IGameNetworkComponent
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

    /// <summary>
    /// 帧同步组件
    /// </summary>
    public interface IGameFrameSynchroComponent : IGameSynchroComponent
    {
        //todo 每帧帧末收集当前玩家的输入，发送给服务器以进行广播
    }

    /// <summary>
    /// 状态同步组件
    /// </summary>
    public interface IGameStateSynchroComponent : IGameSynchroComponent
    {
        //todo 当当前实体对象数据发生改变才进行同步
    }
}
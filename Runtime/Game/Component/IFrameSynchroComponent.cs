namespace ZEngine.Game
{
    /// <summary>
    /// 帧同步组件
    /// </summary>
    public interface IFrameSynchroComponent : ISynchroComponent
    {
        //todo 每帧帧末收集当前玩家的输入，发送给服务器以进行广播
    }
}
namespace ZEngine.Game
{
    /// <summary>
    /// 游戏网络组件
    /// </summary>
    public interface IGameNetworkComponent : IEntityComponent
    {
        /// <summary>
        /// 网络编号
        /// </summary>
        int id { get; }
    }
}
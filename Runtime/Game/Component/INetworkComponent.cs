namespace ZEngine.Game
{
    /// <summary>
    /// 游戏网络组件
    /// </summary>
    public interface INetworkComponent : IComponent
    {
        /// <summary>
        /// 网络编号
        /// </summary>
        int id { get; }
    }
}
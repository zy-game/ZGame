namespace ZGame.Game
{
    /// <summary>
    /// 轮询逻辑系统
    /// </summary>
    public interface IUpdateSystem : ISystem
    {
        void OnUpdate();
    }
}
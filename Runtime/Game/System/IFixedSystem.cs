namespace ZGame.Game
{
    /// <summary>
    /// 按照帧率刷新的逻辑系统
    /// </summary>
    public interface IFixedSystem : ISystem
    {
        void OnFixedUpdate();
    }
}
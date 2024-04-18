namespace ZGame.Game
{
    /// <summary>
    /// 按照帧率刷新的逻辑系统
    /// </summary>
    public interface IFixedSystem : ISystem
    {
        void OnFixedUpdate();
    }

    public interface IResponsiveSystem : ISystem
    {
        void OnRunning(params object[] args);
    }
}
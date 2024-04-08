namespace ZGame.Game
{
    /// <summary>
    /// 帧末轮询逻辑系统
    /// </summary>
    public interface ILateSystem : ISystem
    {
        void OnLateUpdate();
    }
}
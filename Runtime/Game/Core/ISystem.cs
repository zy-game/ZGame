namespace ZGame.Game
{
    /// <summary>
    /// 逻辑系统
    /// </summary>
    public interface ISystem : IReference
    {
        /// <summary>
        /// 系统轮询优先级
        /// </summary>
        uint priority { get; }

        void DoAwake(World world, params object[] args);
    }

    public interface IGizmoSystem : ISystem
    {
        void DoDarwGizmo(World world);
    }

    public interface IUpdateSystem : ISystem
    {
        void DoUpdate(World world);
    }

    public interface IFixedUpdateSystem : ISystem
    {
        void DoFixedUpdate(World world);
    }

    public interface ILateUpdateSystem : ISystem
    {
        void DoLateUpdate(World world);
    }
}
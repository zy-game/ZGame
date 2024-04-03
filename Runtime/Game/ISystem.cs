namespace ZGame.Game
{
    /// <summary>
    /// 逻辑系统
    /// </summary>
    public interface ISystem : IReferenceObject
    {
        /// <summary>
        /// 系统轮询优先级
        /// </summary>
        uint priority { get; }
    }

    /// <summary>
    /// 初始化系统
    /// </summary>
    public interface IInitSystem : ISystem
    {
        void OnAwakw();
    }

    /// <summary>
    /// 轮询逻辑系统
    /// </summary>
    public interface IUpdateSystem : ISystem
    {
        void OnUpdate();
    }

    /// <summary>
    /// 按照帧率刷新的逻辑系统
    /// </summary>
    public interface IFixedUpdateSystem : ISystem
    {
        void OnFixedUpdate();
    }

    /// <summary>
    /// 帧末轮询逻辑系统
    /// </summary>
    public interface ILateUpdateSystem : ISystem
    {
        void OnLateUpdate();
    }

    public interface IGizmoSystem : ISystem
    {
        void OnDrawingGizom();
    }
}
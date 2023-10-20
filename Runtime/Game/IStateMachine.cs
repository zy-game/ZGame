using System;

namespace ZEngine.Game
{
    /// <summary>
    /// 状态机
    /// </summary>
    public interface IStateMachine : IDisposable
    {
        /// <summary>
        /// 状态机名称
        /// </summary>
        string name { get; }

        /// <summary>
        /// 当前状态
        /// </summary>
        IState curState { get; }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="name"></param>
        void SwitchState(string name);

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="state"></param>
        void AddState(IState state);

        /// <summary>
        /// 移除状态
        /// </summary>
        /// <param name="name"></param>
        void RemoveState(string name);

        /// <summary>
        /// 轮询状态机
        /// </summary>
        void Update();
    }
}
using System;

namespace ZEngine.Game
{
    /// <summary>
    /// 状态
    /// </summary>
    public interface IState : IDisposable
    {
        /// <summary>
        /// 状态名
        /// </summary>
        string name { get; }

        /// <summary>
        /// 进入状态
        /// </summary>
        void OnEntry();

        /// <summary>
        /// 轮询状态
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// 退出状态
        /// </summary>
        void OnExit();
    }
}
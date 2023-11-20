using System;

namespace ZGame.State
{
    /// <summary>
    /// 状态逻辑
    /// </summary>
    public interface IStateProcess : IDisposable
    {
        /// <summary>
        /// 初始化状态
        /// </summary>
        void OnAwake();

        /// <summary>
        /// 进入状态
        /// </summary>
        void OnEntry();

        /// <summary>
        /// 退出状态
        /// </summary>
        void OnExit();

        /// <summary>
        /// 轮询状态
        /// </summary>
        void OnUpdate();
    }
}
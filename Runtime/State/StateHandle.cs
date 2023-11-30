using System;

namespace ZGame.State
{
    public abstract class StateHandle : IDisposable
    {
        /// <summary>
        /// 初始化状态
        /// </summary>
        public virtual void OnAwake()
        {
        }

        /// <summary>
        /// 进入状态
        /// </summary>
        public virtual void OnEntry()
        {
        }

        /// <summary>
        /// 退出状态
        /// </summary>
        public virtual void OnExit()
        {
        }

        /// <summary>
        /// 轮询状态
        /// </summary>
        public virtual void OnUpdate()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
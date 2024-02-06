using System;

namespace ZGame
{
    /// <summary>
    /// 命令接收器接口
    /// </summary>
    public interface ICommandExecuter : IDisposable
    {
        /// <summary>
        /// 激活命令接收器
        /// </summary>
        void Awake();

        /// <summary>
        /// 收到命令
        /// </summary>
        /// <param name="args">参数</param>
        void Executer(params object[] args);
    }
}
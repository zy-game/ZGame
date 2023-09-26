using System;

namespace ZEngine.Game
{
    /// <summary>
    /// 逻辑模块
    /// </summary>
    public interface ILogicSystem : IDisposable
    {
        /// <summary>
        /// 初始化游戏逻辑系统
        /// </summary>
        void OnCreate();

        /// <summary>
        /// 移除游戏逻辑系统
        /// </summary>
        void OnDestroy();

        /// <summary>
        /// 轮询游戏逻辑系统
        /// </summary>
        void OnUpdate();
    }
    
    
}
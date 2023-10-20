using System;

namespace ZEngine.Game
{
    /// <summary>
    /// 游戏场景
    /// </summary>
    public interface IGameScene : IDisposable
    {
        /// <summary>
        /// 场景等级
        /// </summary>
        int level { get; }

        /// <summary>
        /// 场景名
        /// </summary>
        string name { get; }

        /// <summary>
        /// 场景配置
        /// </summary>
        ISceneOptions options { get; }

        /// <summary>
        /// 场景初始化
        /// </summary>
        void Awake();

        /// <summary>
        /// 显示场景
        /// </summary>
        void Enable();

        /// <summary>
        /// 隐藏场景
        /// </summary>
        void Disable();
    }
}
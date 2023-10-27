using System;

namespace ZGame.Window
{
    /// <summary>
    /// 游戏窗口界面管理
    /// </summary>
    public interface IWindowSystem : IManager
    {
        /// <summary>
        /// 将窗口置于最前端
        /// </summary>
        /// <param name="type"></param>
        void Focus(Type type);

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IGameWindow Open(Type type);

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IGameWindow GetWindow(Type type);

        /// <summary>
        /// 窗口是否显示或是否打开
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool HasShow(Type type);

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <param name="type"></param>
        void Active(Type type);

        /// <summary>
        /// 窗口失活
        /// </summary>
        /// <param name="type"></param>
        void Inactive(Type type);
    }
}
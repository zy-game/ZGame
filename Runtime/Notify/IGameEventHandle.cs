using System;

namespace ZGame.Notify
{
    /// <summary>
    /// 事件参数
    /// </summary>
    public interface IGameEventArgs : IReference
    {
    }

    /// <summary>
    /// 游戏事件处理管道
    /// </summary>
    public interface IGameEventHandle : IReference
    {
        void Notify(IGameEventArgs args);
    }

    /// <summary>
    /// 游戏事件处理管道
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGameEventHandle<T> : IGameEventHandle
    {
        void Notify(T args);
    }
}
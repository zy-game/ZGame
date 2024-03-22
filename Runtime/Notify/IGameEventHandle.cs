using System;

namespace ZGame.Notify
{
    /// <summary>
    /// 游戏事件处理管道
    /// </summary>
    public interface IGameEventHandle : IDisposable
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
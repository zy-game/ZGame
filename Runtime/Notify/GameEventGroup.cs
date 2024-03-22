using System;
using System.Collections.Generic;

namespace ZGame.Notify
{
    /// <summary>
    /// 游戏事件分组
    /// </summary>
    class GameEventGroup : IDisposable
    {
        private List<IGameEventHandle> _handles = new();

        public void Notify(IGameEventArgs args)
        {
            _handles.ForEach(x => x.Notify(args));
        }

        public void Subscribe(IGameEventHandle handle)
        {
            if (_handles.Exists(x => x.Equals(handle)))
            {
                GameFrameworkEntry.Logger.LogError("重复注册事件：" + handle.ToString());
                return;
            }

            _handles.Add(handle);
        }

        public void Unsubscribe(Action<IGameEventArgs> handle)
        {
            _handles.RemoveAll(x => x.Equals(handle));
        }

        public void Unsubscribe<T>(Action<T> handle) where T : IGameEventArgs
        {
            _handles.RemoveAll(x => x.Equals(handle));
        }

        public void Unsubscribe(IGameEventHandle handle)
        {
            _handles.RemoveAll(x => x.Equals(handle));
        }

        public void Dispose()
        {
            _handles.ForEach(x => x.Dispose());
            _handles.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
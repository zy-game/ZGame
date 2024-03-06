using System;

namespace ZGame.Module
{
    /// <summary>
    /// 模块接口
    /// </summary>
    public interface IModule : IDisposable
    {
        void OnAwake();
    }
}
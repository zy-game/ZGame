using System;

namespace ZEngine.Window
{
    public interface ICountdownPipeline : IDisposable
    {
        float progres { get; }
        float time { get; }
        float interval { get; }
        void Active();
        void Inactive();
        void Reset();
    }
}
using System;

namespace ZEngine
{
    public interface IGameProgressHandle : IDisposable
    {
        void SetTextInfo(string text);
        void SetProgress(float progress);
        void SetTextAndProgress(string text, float progress);
    }
}
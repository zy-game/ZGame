using System;

namespace ZEngine
{
    public interface IProgressHandle : IDisposable
    {
        void SetTextInfo(string text);
        void SetProgress(float progress);
    }
}
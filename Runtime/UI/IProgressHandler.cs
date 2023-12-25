using System;

namespace UI
{
    public interface IProgressHandler : IProgress<float>
    {
        void SetTitle(string title);
    }
}
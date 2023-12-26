using System;
using ZGame.Window;

namespace UI
{
    public interface ILoading : UIBase, IProgress<float>
    {
        void SetTitle(string title);
    }
}
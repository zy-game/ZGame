using System;

namespace ZEngine.Window
{
    public interface IWindowHandle
    {
        void SetText(string text);
    }

    public interface ITimwoutWindow : IWindowHandle
    {
        void Timeout(float time);
    }

    public interface IToastWindowHandle : IWindowHandle
    {
    }

    public interface IWaitWindowHandle : ITimwoutWindow
    {
    }

    public interface IMessageBoxWindowHandle : IWindowHandle
    {
        void SetCancel(Action action);
        void SetEntry(Action action);
    }
}
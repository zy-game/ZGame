using System;

namespace ZEngine.Window
{
    public interface IOpenedWindowExecuteHandle<T> : IExecuteAsyncHandle<IOpenedWindowExecuteHandle<T>>
    {
    }

    public class WindowManager : Single<WindowManager>
    {
        public IWindowHandle OpenWindow(Type windowType)
        {
            return default;
        }

        public IOpenedWindowExecuteHandle<IWindowHandle> OpenWindowAsync(Type windowType)
        {
            return default;
        }

        public IOpenedWindowExecuteHandle<T> OpenWindowAsync<T>() where T : IWindowHandle
        {
            return default;
        }

        public IWindowHandle GetWindow(Type windowType)
        {
            return default;
        }

        public void Close(Type windowType)
        {
        }
    }
}
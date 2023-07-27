using System;

namespace ZEngine.Window
{
    public class WindowManager : Single<WindowManager>
    {
        public IWindowHandle OpenWindow(Type windowType)
        {
            return default;
        }

        public IGameAsyncExecuteHandle<IWindowHandle> OpenWindowAsync(Type windowType)
        {
            return default;
        }

        public IGameAsyncExecuteHandle<T> OpenWindowAsync<T>() where T : IWindowHandle
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
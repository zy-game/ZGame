using System;

namespace ZEngine.Window
{
    public interface IOpenedWindowExecuteAsyncHandleHandle<T> : IExecuteHandle<IOpenedWindowExecuteAsyncHandleHandle<T>>
    {
    }

    public class WindowManager : Single<WindowManager>
    {
        public IWindowHandle OpenWindow(Type windowType)
        {
            return default;
        }

        public IOpenedWindowExecuteAsyncHandleHandle<IWindowHandle> OpenWindowAsync(Type windowType)
        {
            return default;
        }

        public IOpenedWindowExecuteAsyncHandleHandle<T> OpenWindowAsync<T>() where T : IWindowHandle
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
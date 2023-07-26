using System;
using Cysharp.Threading.Tasks;

namespace ZEngine.Window
{
    public class WindowManager : Single<WindowManager>
    {
        public IWindowHandle OpenWindow(Type windowType)
        {
            return default;
        }

        public UniTask<IWindowHandle> OpenWindowAsync(Type windowType)
        {
            return default;
        }

        public UniTask<T> OpenWindowAsync<T>() where T : IWindowHandle
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
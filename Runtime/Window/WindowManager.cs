using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Window
{
    public interface IOpenedWindowExecuteHandle<T> : IExecuteHandle<IOpenedWindowExecuteHandle<T>>
    {
    }


    public class WindowManager : Single<WindowManager>
    {
        private Dictionary<Type, UIWindow> cache = new Dictionary<Type, UIWindow>();
        private Dictionary<Type, UIWindow> windows = new Dictionary<Type, UIWindow>();

        public UIWindow OpenWindow(Type windowType)
        {
            if (windows.TryGetValue(windowType, out UIWindow window))
            {
                return window;
            }

            UIOptions options = windowType.GetCustomAttribute<UIOptions>();
            if (options is null)
            {
                Engine.Console.Error(EngineException.Create<ArgumentNullException>(nameof(UIOptions)));
                return default;
            }

            IAssetRequestExecute<GameObject> request = Engine.Resource.LoadAsset<GameObject>(options.path);
            if (request.result == null)
            {
                Engine.Console.Error(EngineException.Create<NullReferenceException>(options.path));
                return default;
            }

            window = (UIWindow)Activator.CreateInstance(windowType);
            window.SetGameObject(GameObject.Instantiate(request.result));
            window.OnAwake();
            window.OnEnable();
            return window;
        }

        public UIWindow GetWindow(Type windowType)
        {
            if (windows.TryGetValue(windowType, out UIWindow window))
            {
                return window;
            }

            return default;
        }

        public void Close(Type windowType, bool isCache)
        {
            UIWindow window = GetWindow(windowType);
            if (window is null)
            {
                return;
            }

            if (isCache)
            {
                window.OnDiable();
                cache.Add(windowType, window);
            }
            else
            {
                Engine.Class.Release(window);
            }
        }
    }
}
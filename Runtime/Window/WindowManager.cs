using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Window
{
    class WindowManager : Singleton<WindowManager>
    {
        private Dictionary<Type, UIWindow> windows = new Dictionary<Type, UIWindow>();

        public UIWindow OpenWindow(Type windowType)
        {
            if (windows.TryGetValue(windowType, out UIWindow window))
            {
                return window;
            }

            if (ZGame.Cache.TryGetValue(windowType.Name, out window))
            {
                window.gameObject.SetActive(true);
                window.Enable();
                windows.Add(windowType, window);
                return window;
            }

            UIOptions options = windowType.GetCustomAttribute<UIOptions>();
            if (options is null)
            {
                ZGame.Console.Error(new ArgumentNullException(nameof(UIOptions)));
                return default;
            }

            IRequestResourceObjectResult requestResourceObjectResult = ZGame.Resource.LoadAsset(options.path);
            if (requestResourceObjectResult.EnsureLoadAssetSuccessfuly() is false)
            {
                ZGame.Console.Error(new NullReferenceException(options.path));
                return default;
            }

            window = (UIWindow)Activator.CreateInstance(windowType);
            window.gameObject = requestResourceObjectResult.Instantiate();
            UILayerManager.instance.SetLayer((byte)options.layer, window.gameObject);
            windows.Add(windowType, window);
            window.Awake();
            window.Enable();
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
                ZGame.Console.Error("未找到指定类型的window:", windowType, windows.Count);
                return;
            }

            Hide(windowType);
            windows.Remove(windowType);
            if (isCache)
            {
                ZGame.Cache.Enqueue(windowType.Name, window);
            }
            else
            {
                window.Dispose();
            }
        }

        public void Show(Type type)
        {
            UIWindow window = GetWindow(type);
            if (window is null)
            {
                ZGame.Console.Log("Not Find Window Type:", type.Name);
                return;
            }

            window.gameObject.SetActive(true);
            window.Enable();
        }

        public void Hide(Type type)
        {
            UIWindow window = GetWindow(type);
            if (window is null)
            {
                ZGame.Console.Log("Not Find Window Type:", type.Name);
                return;
            }

            window.gameObject.SetActive(false);
            window.Disable();
        }

        public void OnEvent(Type type, string name, params object[] args)
        {
            if (type is null)
            {
                foreach (var VARIABLE in windows.Values)
                {
                    VARIABLE.OnEvent(name, args);
                }

                return;
            }

            if (windows.TryGetValue(type, out UIWindow window) is false)
            {
                return;
            }

            window.OnEvent(name, args);
        }

        public void Clear()
        {
            foreach (var VARIABLE in windows.Values)
            {
                VARIABLE.Dispose();
            }

            windows.Clear();
            ZGame.Cache.RemoveCacheArea<UIWindow>();
        }

        public override void Dispose()
        {
            Clear();
            UILayerManager.instance.Clear();
        }
    }
}
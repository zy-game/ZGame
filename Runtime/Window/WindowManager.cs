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
        private Dictionary<UIOptions.Layer, Canvas> canvasMap = new Dictionary<UIOptions.Layer, Canvas>();

        public override void Dispose()
        {
            base.Dispose();
            foreach (var VARIABLE in windows.Values)
            {
                VARIABLE.Dispose();
            }

            foreach (var VARIABLE in canvasMap.Values)
            {
                GameObject.DestroyImmediate(VARIABLE.gameObject);
            }

            ZGame.Cache.RemoveCacheArea<UIWindow>();
            windows.Clear();
            canvasMap.Clear();
            ZGame.Console.Log("关闭所有窗口");
        }

        public UIWindow OpenWindow(Type windowType)
        {
            if (windows.TryGetValue(windowType, out UIWindow window))
            {
                return window;
            }

            if (ZGame.Cache.TryGetValue(windowType.Name, out window))
            {
                window.OnEnable();
                windows.Add(windowType, window);
                return window;
            }

            UIOptions options = windowType.GetCustomAttribute<UIOptions>();
            if (options is null)
            {
                ZGame.Console.Error(new ArgumentNullException(nameof(UIOptions)));
                return default;
            }

            IRequestAssetObjectResult requestAssetObjectResult = ZGame.Resource.LoadAsset(options.path);
            if (requestAssetObjectResult.result == null)
            {
                ZGame.Console.Error(new NullReferenceException(options.path));
                return default;
            }

            window = (UIWindow)Activator.CreateInstance(windowType);
            window.SetGameObject(requestAssetObjectResult.Instantiate(), IUIWindowOptions.Create(options.localization));
            UILayerManager.instance.SetLayer((byte)options.layer, window.gameObject);
            windows.Add(windowType, window);
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
                ZGame.Console.Error("未找到指定类型的window:", windowType, windows.Count);
                return;
            }

            Hide(windowType);
            windows.Remove(windowType);
            if (isCache)
            {
                ZGame.Cache.Handle(windowType.Name, window);
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

            window.OnEnable();
        }

        public void Hide(Type type)
        {
            UIWindow window = GetWindow(type);
            if (window is null)
            {
                ZGame.Console.Log("Not Find Window Type:", type.Name);
                return;
            }

            window.OnDiable();
        }
    }
}
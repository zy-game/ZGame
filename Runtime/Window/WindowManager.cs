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
        private List<CacheData> cacheList = new List<CacheData>();
        private Dictionary<Type, UIWindow> windows = new Dictionary<Type, UIWindow>();

        class CacheData : IReference
        {
            public float time;
            public UIWindow handle;


            public void Release()
            {
                throw new NotImplementedException();
            }

            public static CacheData Create(UIWindow handle)
            {
                CacheData cacheData = new CacheData();
                cacheData.handle = handle;
                cacheData.time = Time.realtimeSinceStartup + HotfixOptions.instance.cachetime;
                return cacheData;
            }
        }

        public UIWindow OpenWindow(Type windowType)
        {
            if (windows.TryGetValue(windowType, out UIWindow window))
            {
                return window;
            }

            CacheData cacheData = cacheList.Find(x => x.handle.GetType() == windowType);
            if (cacheData is not null)
            {
                cacheList.Remove(cacheData);
                window = cacheData.handle;
                window.OnEnable();
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
            windows.Add(windowType, window);
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

            windows.Remove(windowType);
            if (isCache)
            {
                window.OnDiable();
                cacheList.Add(CacheData.Create(window));
            }
            else
            {
                Engine.Class.Release(window);
            }
        }
    }
}
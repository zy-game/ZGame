using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
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

            Launche.Cache.RemoveCacheArea<UIWindow>();
            windows.Clear();
            canvasMap.Clear();
            Launche.Console.Log("关闭所有窗口");
        }

        public UIWindow OpenWindow(Type windowType)
        {
            if (windows.TryGetValue(windowType, out UIWindow window))
            {
                return window;
            }

            if (Launche.Cache.TryGetValue(windowType.Name, out window))
            {
                window.OnEnable();
                windows.Add(windowType, window);
                return window;
            }

            UIOptions options = windowType.GetCustomAttribute<UIOptions>();
            if (options is null)
            {
                Launche.Console.Error(new ArgumentNullException(nameof(UIOptions)));
                return default;
            }

            IRequestAssetObjectResult<GameObject> requestAssetObjectResult = Launche.Resource.LoadAsset<GameObject>(options.path);
            if (requestAssetObjectResult.result == null)
            {
                Launche.Console.Error(new NullReferenceException(options.path));
                return default;
            }

            window = (UIWindow)Activator.CreateInstance(windowType);
            window.SetGameObject(requestAssetObjectResult.Instantiate());
            SetToLayer(options.layer, window.gameObject);
            windows.Add(windowType, window);
            Launche.Console.Log("Create Window:", windowType);
            window.OnAwake();
            Show(windowType);
            return window;
        }


        public void SetToLayer(UIOptions.Layer layer, GameObject gameObject)
        {
            if (!canvasMap.TryGetValue(layer, out Canvas canvas))
            {
                canvas = new GameObject("Canvas").AddComponent<Canvas>();
                CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                canvas.sortingOrder = (int)layer;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.additionalShaderChannels = 0 >> 0;
                canvas.gameObject.layer = 5;
                canvasMap.Add(layer, canvas);
                GameObject.DontDestroyOnLoad(canvas.gameObject);
            }

            gameObject.transform.SetParent(canvas.transform);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
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
                Launche.Console.Error("未找到指定类型的window:", windowType, windows.Count);
                return;
            }

            Hide(windowType);
            windows.Remove(windowType);
            if (isCache)
            {
                Launche.Cache.Handle(windowType.Name, window);
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
                Launche.Console.Log("Not Find Window Type:", type.Name);
                return;
            }

            window.OnEnable();
        }

        public void Hide(Type type)
        {
            UIWindow window = GetWindow(type);
            if (window is null)
            {
                Launche.Console.Log("Not Find Window Type:", type.Name);
                return;
            }

            window.OnDiable();
        }
    }
}
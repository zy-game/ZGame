using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using ZEngine.Resource;

namespace ZEngine.Window
{
    public class WindowManager : Single<WindowManager>
    {
        private List<CacheData> cacheList = new List<CacheData>();
        private Dictionary<Type, UIWindow> windows = new Dictionary<Type, UIWindow>();
        private Dictionary<UIOptions.Layer, Canvas> canvasMap = new Dictionary<UIOptions.Layer, Canvas>();

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

        public override void Dispose()
        {
            base.Dispose();
            Engine.Console.Log("关闭所有窗口");
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

            IRequestAssetExecuteResult<GameObject> request = Engine.Resource.LoadAsset<GameObject>(options.path);
            if (request.asset == null)
            {
                Engine.Console.Error(EngineException.Create<NullReferenceException>(options.path));
                return default;
            }

            window = (UIWindow)Activator.CreateInstance(windowType);
            Engine.Console.Log("Create Window:", windowType.Name);
            window.SetGameObject(GameObject.Instantiate(request.asset));
            SetToLayer(options.layer, window.gameObject);
            windows.Add(windowType, window);
            window.OnAwake();
            window.OnEnable();
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
                Engine.Console.Log("Not Find Window Type:", windowType.Name);
                return;
            }

            windows.Remove(windowType);
            window.OnDiable();
            if (isCache)
            {
                cacheList.Add(CacheData.Create(window));
            }
            else
            {
                Engine.Class.Release(window);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Resource;

namespace ZGame.UI
{
    class UIRoot : IDisposable
    {
        public int layer;
        private Canvas canvas;
        private List<UIBase> uiList = new();

        public UIRoot(int layer)
        {
            this.layer = layer;
            Init();
        }

        public UIRoot(UILayer layer) : this((int)layer)
        {
            this.canvas.name = "UIRoot_" + layer;
        }

        private void Init()
        {
            canvas = new GameObject("UIRoot_" + layer).AddComponent<Canvas>();
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            canvas.sortingOrder = layer;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.gameObject.layer = 5;
            canvas.additionalShaderChannels = 0;
            GameObject.DontDestroyOnLoad(canvas.gameObject);
        }

        public void Dispose()
        {
            foreach (var VARIABLE in uiList)
            {
                VARIABLE.Dispose();
            }

            uiList.Clear();
            GameObject.DestroyImmediate(canvas.gameObject);
            canvas = null;
            GC.SuppressFinalize(this);
        }

        public bool Contains(Type type)
        {
            return uiList.Exists(x => x.GetType() == type);
        }

        public UIBase Active(UIOptions options, Type type, params object[] args)
        {
            UIBase uiBase = uiList.Find(x => x.GetType() == type);
            if (uiBase is not null)
            {
                uiBase.Enable(args);
                return uiBase;
            }

            ResourceReference reference = type.GetCustomAttribute<ResourceReference>();
            if (reference is null || reference.path.IsNullOrEmpty())
            {
                Debug.LogError("没找到资源引用:" + type.Name);
                return default;
            }

            ResObject resObject = ResourceManager.instance.LoadAsset(reference.path);
            if (resObject.IsSuccess() is false)
            {
                Debug.Log("加载资源失败：" + reference.path);
                return default;
            }

            uiBase = (UIBase)Activator.CreateInstance(type, new object[] { resObject.Instantiate() });
            uiBase.gameObject.transform.SetParent(canvas.transform);
            uiBase.gameObject.transform.position = Vector3.zero;
            uiBase.gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
            uiBase.gameObject.transform.localScale = Vector3.one;
            if (uiBase.gameObject.TryGetComponent<RectTransform>(out RectTransform rectTransform))
            {
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }

            if (options.sceneType == SceneType.Overlap)
            {
                uiList.ForEach(x => x.Disable());
            }

            uiList.Add(uiBase);
            uiBase.Awake();
            uiBase.gameObject.transform.SetAsLastSibling();
            uiBase.Enable(args);
            return uiBase;
        }

        public void Inactive(Type type)
        {
            UIOptions options = type.GetCustomAttribute<UIOptions>();
            if (options is null)
            {
                Debug.LogError("没有找到UIOptions:" + type);
                return;
            }

            UIBase uiBase = uiList.Find(x => x.GetType() == type);
            if (uiBase == null)
            {
                Debug.LogError("没有找到指定的UI:" + type);
                return;
            }

            uiBase.Disable();
            uiBase.gameObject.transform.SetAsFirstSibling();
            if (options.cacheType == CacheType.Temp)
            {
                uiBase.Dispose();
                uiList.Remove(uiBase);
            }
        }

        public void Disable()
        {
            for (int i = 0; i < uiList.Count; i++)
            {
                uiList[i].Disable();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.UI
{
    class UIRoot : IReferenceObject
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

        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWindow<T>() where T : UIBase
        {
            return (T)GetWindow(typeof(T));
        }

        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public UIBase GetWindow(Type type)
        {
            if (typeof(UIBase).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(type));
            }

            return uiList.Find(x => x.GetType() == type);
        }

        public bool Contains(Type type)
        {
            return uiList.Exists(x => x.GetType() == type);
        }

        public UIBase Active(UIOptions options, Type type, params object[] args)
        {
            UIBase uiBase = uiList.Find(x => x.GetType() == type);
            if (uiBase is null)
            {
                RefPath reference = type.GetCustomAttribute<RefPath>();
                if (reference is null || reference.path.IsNullOrEmpty())
                {
                    GameFrameworkEntry.Logger.LogError("没找到资源引用:" + type.Name);
                    return default;
                }


                uiBase = UIBase.Create(reference.path, type, canvas);
                uiList.Add(uiBase);
                uiBase.Awake();
            }

            if (options.sceneType == SceneType.Overlap)
            {
                uiList.ForEach(x => x.Disable());
            }

            uiBase.Enable(args);
            uiBase.gameObject.transform.SetAsLastSibling();
            return uiBase;
        }

        public void Inactive(Type type)
        {
            UIOptions options = type.GetCustomAttribute<UIOptions>();
            if (options is null)
            {
                GameFrameworkEntry.Logger.LogError("没有找到UIOptions:" + type);
                return;
            }

            UIBase uiBase = uiList.Find(x => x.GetType() == type);
            if (uiBase == null)
            {
                GameFrameworkEntry.Logger.LogError("没有找到指定的UI:" + type);
                return;
            }

            uiBase.Disable();
            uiBase.gameObject.transform.SetAsFirstSibling();
            if (options.cacheType == CacheType.Temp)
            {
                GameFrameworkFactory.Release(uiBase);
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

        public void Release()
        {
            foreach (var VARIABLE in uiList)
            {
                GameFrameworkFactory.Release(VARIABLE);
            }

            uiList.Clear();
            GameObject.DestroyImmediate(canvas.gameObject);
            canvas = null;
        }
    }
}
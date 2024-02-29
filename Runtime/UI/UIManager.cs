using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZGame.Game;
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


        public UIBase AddChild(LOADTYPE loadtype, Type type, Vector3 pos, Vector3 rot, Vector3 scale)
        {
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

            UIBase uiBase = (UIBase)Activator.CreateInstance(type, new object[] { resObject.Instantiate() });
            uiBase.gameObject.transform.SetParent(canvas.transform);
            uiBase.gameObject.transform.position = pos;
            uiBase.gameObject.transform.rotation = Quaternion.Euler(rot);
            uiBase.gameObject.transform.localScale = scale;
            if (uiBase.gameObject.TryGetComponent<RectTransform>(out RectTransform rectTransform))
            {
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }

            if (loadtype == LOADTYPE.OVERLAP)
            {
                uiList.ForEach(x => x.Disable());
            }

            uiList.Add(uiBase);
            uiBase.Awake();
            return uiBase;
        }

        public UIBase Back(Type type, bool dispose, params object[] args)
        {
            UIOptions options = type.GetCustomAttribute<UIOptions>();
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                if ((options.parent is not null && uiList[i].GetType() == options.parent) || i == 0)
                {
                    break;
                }

                Inactive(uiList[i].GetType(), dispose);
            }

            return Active(options.parent);
        }

        public UIBase Active(Type type, params object[] args)
        {
            UIBase uiBase = uiList.Find(x => x.GetType() == type);
            if (uiBase is null)
            {
                return uiBase;
            }

            Debug.Log("active :" + uiBase.GetType());
            uiBase.gameObject.SetActive(true);
            uiBase.Enable(args);
            return uiBase;
        }

        public UIBase Inactive(Type type, bool dispose)
        {
            UIBase uiBase = uiList.Find(x => x.GetType() == type);
            if (uiBase == null)
            {
                return uiBase;
            }

            uiBase.gameObject.SetActive(false);
            uiBase.Disable();
            if (dispose)
            {
                uiBase.Dispose();
                uiList.Remove(uiBase);
            }

            UIOptions options = type.GetCustomAttribute<UIOptions>();
            if (options is null)
            {
                return default;
            }


            return uiBase;
        }

        public void Close(Type type, bool dispose)
        {
            UIBase uiBase = uiList.Find(x => x.GetType() == type);
            if (uiBase == null)
            {
                return;
            }

            uiBase.Disable();
            if (dispose)
            {
                uiBase.Dispose();
                uiList.Remove(uiBase);
            }
        }
    }

    /// <summary>
    /// 界面管理器
    /// </summary>
    public sealed class UIManager : Singleton<UIManager>
    {
        private UIBase current;
        private List<UIRoot> rootList = new();


        protected override void OnAwake()
        {
            BehaviourScriptable.instance.gameObject.AddComponent<EventSystem>();
            BehaviourScriptable.instance.gameObject.AddComponent<StandaloneInputModule>();
            GetOrCreateCanvas((int)UILAYER.BACKGROUND);
            GetOrCreateCanvas((int)UILAYER.BOTTOM);
            GetOrCreateCanvas((int)UILAYER.MIDDLE);
            GetOrCreateCanvas((int)UILAYER.TOP);
            GetOrCreateCanvas((int)UILAYER.LOADING);
            GetOrCreateCanvas((int)UILAYER.MESSAGE);
            GetOrCreateCanvas((int)UILAYER.WAITING);
            GetOrCreateCanvas((int)UILAYER.TIPS);
        }

        private UIRoot GetOrCreateCanvas(int layer)
        {
            UIRoot root = rootList.Find(x => x.layer == layer);
            if (root is null)
            {
                rootList.Add(root = new UIRoot(layer));
            }

            return root;
        }

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Open<T>(params object[] args) where T : UIBase
        {
            return (T)Open(typeof(T), args);
        }

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public UIBase Open(Type type, params object[] args)
        {
            if (type is null || type.IsInterface || type.IsAbstract || type.IsSubclassOf(typeof(UIBase)) is false)
            {
                Debug.LogError("创建UI失败");
                return default;
            }

            UIRoot root = rootList.Find(x => x.Contains(type));
            if (root is not null)
            {
                return root.Active(type, args);
            }

            UIOptions options = type.GetCustomAttribute<UIOptions>();
            if (options is null)
            {
                Debug.LogError("没找到UIOptions:" + type.Name);
                return default;
            }

            if (options.parent is null)
            {
                root = rootList.Find(x => x.layer == options.layer);
            }
            else
            {
                root = rootList.Find(x => x.Contains(options.parent));
            }

            if (root is null)
            {
                Debug.LogError("没有找到父节点：" + options.parent.Name);
                return default;
            }

            UIBase uiBase = root.AddChild(options.loadtype, type, Vector3.zero, Vector3.zero, Vector3.one);
            if (uiBase is null)
            {
                Debug.LogError("创建UI失败");
                return default;
            }

            return current = root.Active(type, args);
        }

        public void Back(bool dipose = false, params object[] args)
        {
            if (current is null)
            {
                return;
            }

            UIRoot root = rootList.Find(x => x.Contains(current.GetType()));
            if (root is null)
            {
                Debug.LogError("没有找到父节点：" + current.GetType().Name);
                return;
            }

            current = root.Back(current.GetType(), dipose, args);
        }

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Active<T>(params object[] args)
        {
            Active(typeof(T), args);
        }

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <param name="type"></param>
        public void Active(Type type, params object[] args)
        {
            if (type is null)
            {
                Debug.LogError(new NullReferenceException(nameof(type)));
                return;
            }

            UIRoot root = rootList.Find(x => x.Contains(type));
            if (root is null)
            {
                Debug.LogError("没有找到父节点：" + type.Name);
                return;
            }

            current = root.Active(type, args);
        }

        /// <summary>
        /// 窗口失活
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Inactive<T>()
        {
            Inactive(typeof(T));
        }

        /// <summary>
        /// 失活活UI
        /// </summary>
        /// <param name="uiBase"></param>
        public void Inactive(UIBase uiBase)
        {
            if (uiBase is null)
            {
                Debug.Log("ui is null");
                return;
            }

            Inactive(uiBase.GetType());
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <param name="type"></param>
        public void Inactive(Type type)
        {
            if (type is null)
            {
                Debug.Log(new NullReferenceException(type.Name));
                return;
            }

            UIRoot root = rootList.Find(x => x.Contains(type));
            if (root is null)
            {
                Debug.LogError("没有找到父节点：" + type.Name);
                return;
            }

            root.Inactive(type, false);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="dispose">是否释放UI</param>
        /// <typeparam name="T">UI类型</typeparam>
        public void Close<T>(bool dispose = true)
        {
            Close(typeof(T), dispose);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="type">UI类型</param>
        /// <param name="dispose">是否释放UI</param>
        public void Close(UIBase uiBase, bool dispose = true)
        {
            if (uiBase is null)
            {
                Debug.LogError(nameof(uiBase));
                return;
            }

            Close(uiBase.GetType(), dispose);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="type">UI类型</param>
        /// <param name="dispose">是否释放UI</param>
        public void Close(Type type, bool dispose = true)
        {
            if (type is null)
            {
                Debug.LogError(nameof(type));
                return;
            }

            UIRoot root = rootList.Find(x => x.Contains(type));
            if (root is null)
            {
                Debug.LogError("没有找到父节点：" + type.Name);
                return;
            }

            root.Close(type, dispose);
        }

        /// <summary>
        /// 清理所有UI
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < rootList.Count; i++)
            {
                rootList[i].Dispose();
            }

            rootList.Clear();
        }
    }
}
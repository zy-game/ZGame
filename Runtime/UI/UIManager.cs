using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Game;
using ZGame.Resource;

namespace ZGame.UI
{
    /// <summary>
    /// 界面管理器
    /// </summary>
    public sealed class UIManager : Singleton<UIManager>
    {
        // private List<UIBase> _windows = new();
        private List<UIBase> uiQueue = new();
        private List<UIBase> uiCache = new();

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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Open<T>(string resPath, params object[] args) where T : UIBase
        {
            return (T)Open(typeof(T), resPath, args);
        }

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public UIBase Open(Type type, params object[] args)
        {
            if (type is null || type.IsInterface || type.IsAbstract || typeof(UIBase).IsAssignableFrom(type) is false)
            {
                return default;
            }

            UIBase uiBase = GetWindow(type);
            if (uiBase is not null)
            {
                Active(uiBase, args);
                return uiBase;
            }

            ResourceReference reference = type.GetCustomAttribute<ResourceReference>();
            if (reference is null || reference.path.IsNullOrEmpty())
            {
                Debug.LogError("没找到资源引用:" + type.Name);
                return default;
            }

            return Open(type, reference.path, args);
        }

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public UIBase Open(Type type, string resPath, params object[] args)
        {
            if (type is null || typeof(UIBase).IsAssignableFrom(type) is false || type.IsInterface || type.IsAbstract)
            {
                return default;
            }

            UIBase uiBase = GetWindow(type);
            if (uiBase is not null)
            {
                Active(uiBase, args);
                return uiBase;
            }

            ResObject resObject = ResourceManager.instance.LoadAsset(resPath);
            if (resObject.IsSuccess() is false)
            {
                Debug.Log("加载资源失败：" + resPath);
                return default;
            }

            uiBase = (UIBase)Activator.CreateInstance(type, new object[] { resObject.Instantiate() });
            UILayers.instance.TrySetup(uiBase.gameObject, 1, Vector3.zero, Vector3.zero, Vector3.one);
            uiBase.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            uiBase.Awake();
            Active(uiBase, args);
            return uiBase;
        }

        public void BackHome(params object[] args)
        {
            for (int i = uiQueue.Count - 1; i >= 0; i--)
            {
                var VARIABLE = uiQueue[i];
                HomeUI homeUI = VARIABLE.GetType().GetCustomAttribute<HomeUI>();
                if (homeUI is not null)
                {
                    Active(VARIABLE, args);
                    break;
                }

                Inactive(VARIABLE);
            }

            for (int i = uiCache.Count - 1; i >= 0; i--)
            {
                var VARIABLE = uiCache[i];
                HomeUI homeUI = VARIABLE.GetType().GetCustomAttribute<HomeUI>();
                if (homeUI is not null)
                {
                    Active(VARIABLE, args);
                    break;
                }
            }
        }

        public void Back(params object[] args)
        {
            UIBase top = uiQueue.LastOrDefault();
            if (top is null)
            {
                return;
            }

            Inactive(top);
            Debug.Log(string.Join(",", uiQueue.Select(x => x.name)));
            if (uiQueue.Count > 0)
            {
                Active(uiQueue.LastOrDefault(), args);
                return;
            }

            top = uiCache.LastOrDefault();
            if (top is null)
            {
                return;
            }

            Active(top, args);
        }

        /// <summary>
        /// 是否打开窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasOpened<T>() where T : UIBase
        {
            return HasOpened(typeof(T));
        }

        /// <summary>
        /// 是否打开窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasOpened(Type type)
        {
            return GetWindow(type) is not null;
        }

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWindow<T>() where T : UIBase
        {
            return (T)GetWindow(typeof(T));
        }

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public UIBase GetWindow(Type type)
        {
            if (type is null || typeof(UIBase).IsAssignableFrom(type) is false)
            {
                return default;
            }

            UIBase temp = uiQueue.Find(x => type.IsAssignableFrom(x.GetType()));
            if (temp is null)
            {
                temp = uiCache.Find(x => type.IsAssignableFrom(x.GetType()));
            }

            return temp;
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
            Active(GetWindow(type), args);
        }

        /// <summary>
        /// 激活UI
        /// </summary>
        /// <param name="uiBase"></param>
        public void Active(UIBase uiBase, params object[] args)
        {
            if (uiBase is null)
            {
                return;
            }

            uiQueue.Remove(uiBase);
            uiQueue.Add(uiBase);
            uiCache.Remove(uiBase);
            uiBase.gameObject.SetActive(true);
            uiBase.Enable(args);
            Debug.Log(string.Join(",", uiQueue.Select(x => x.name)));
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
        /// 隐藏窗口
        /// </summary>
        /// <param name="type"></param>
        public void Inactive(Type type)
        {
            Inactive(GetWindow(type));
        }

        /// <summary>
        /// 失活活UI
        /// </summary>
        /// <param name="uiBase"></param>
        public void Inactive(UIBase uiBase)
        {
            if (uiBase is null)
            {
                return;
            }

            uiQueue.Remove(uiBase);
            uiCache.Remove(uiBase);
            uiCache.Add(uiBase);
            uiBase.gameObject.SetActive(false);
            uiBase.Disable();
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
        public void Close(Type type, bool dispose = true)
        {
            UIBase uiBase = GetWindow(type);
            if (uiBase is null)
            {
                return;
            }

            uiBase.Disable();
            uiQueue.Remove(uiBase);
            if (dispose is false)
            {
                uiCache.Add(uiBase);
                return;
            }

            uiBase.Dispose();
            GameObject.DestroyImmediate(uiBase.gameObject);
        }

        /// <summary>
        /// 清理所有UI
        /// </summary>
        public void Clear()
        {
            foreach (var VARIABLE in uiQueue)
            {
                VARIABLE.Disable();
                VARIABLE.Dispose();
                GameObject.DestroyImmediate(VARIABLE.gameObject);
            }

            foreach (var VARIABLE in uiCache)
            {
                VARIABLE.Disable();
                VARIABLE.Dispose();
                GameObject.DestroyImmediate(VARIABLE.gameObject);
            }

            uiQueue.Clear();
            uiCache.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Game;
using ZGame.Resource;

namespace ZGame.Window
{
    /// <summary>
    /// 界面管理器
    /// </summary>
    public sealed class UIManager : Singleton<UIManager>
    {
        private List<UIBase> _windows = new();
        private Dictionary<Type, Type> basicTypes = new();

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
            UIBase uiBase = GetWindow(type);
            if (uiBase is not null)
            {
                return uiBase;
            }

            type = GetUIType(type);

            if (type is null)
            {
                return default;
            }

            ResHandle resObject = GetResHandle(type);
            if (resObject is null)
            {
                return default;
            }

            Debug.Log("加载UI：" + type.Name);
            uiBase = (UIBase)Activator.CreateInstance(type, new object[] { resObject.Instantiate() });
            UILayers.instance.TrySetup(uiBase.gameObject, 1, Vector3.zero, Vector3.zero, Vector3.one);
            uiBase.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            _windows.Add(uiBase);
            uiBase.Awake();
            Active(type);
            return uiBase;
        }

        private ResHandle GetResHandle(Type type)
        {
            ResourceReference reference = type.GetCustomAttribute<ResourceReference>();
            if (reference is null || reference.path.IsNullOrEmpty())
            {
                Debug.LogError("没找到资源引用:" + type.Name);
                return default;
            }

            return ResourceManager.instance.LoadAsset(reference.path);
        }

        private Type GetUIType(Type type)
        {
            if (type.IsInterface || type.IsAbstract)
            {
                if (basicTypes.TryGetValue(type, out Type basicType) is false)
                {
                    basicType = AppDomain.CurrentDomain.GetAllSubClasses(type).FirstOrDefault();
                    basicTypes.Add(type, basicType);
                }

                type = basicType;
            }

            return type;
        }

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWindow<T>()
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
            if (type is null)
            {
                return default;
            }

            return _windows.Find(x => type.IsAssignableFrom(x.GetType()));
        }

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Active<T>()
        {
            Active(typeof(T));
        }

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <param name="type"></param>
        public void Active(Type type)
        {
            UIBase uiBase = GetWindow(type);
            if (uiBase is null || uiBase.gameObject.activeSelf)
            {
                return;
            }

            uiBase.gameObject.SetActive(true);
            uiBase.Enable();
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
            UIBase uiBase = GetWindow(type);
            if (uiBase is null || uiBase.gameObject.activeSelf is false)
            {
                return;
            }

            uiBase.gameObject.SetActive(false);
            uiBase.Disable();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="system"></param>
        /// <typeparam name="T"></typeparam>
        public void Close<T>(bool dispose = true)
        {
            Close(typeof(T), dispose);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="type"></param>
        public void Close(Type type, bool dispose = true)
        {
            UIBase uiBase = GetWindow(type);
            if (uiBase is null)
            {
                return;
            }

            uiBase.Disable();
            uiBase.Dispose();
            GameObject.DestroyImmediate(uiBase.gameObject);
            _windows.Remove(uiBase);
        }

        /// <summary>
        /// 清理所有UI
        /// </summary>
        public void Clear()
        {
            foreach (var VARIABLE in _windows)
            {
                VARIABLE.Disable();
                VARIABLE.Dispose();
                GameObject.DestroyImmediate(VARIABLE.gameObject);
            }

            _windows.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using ZGame.Game;
using ZGame.Module;

namespace ZGame.UI
{
    /// <summary>
    /// 界面管理器
    /// </summary>
    public sealed class UIManager : IModule
    {
        private List<UIRoot> rootList;

        public void OnAwake()
        {
            BehaviourScriptable.instance.gameObject.AddComponent<EventSystem>();
            BehaviourScriptable.instance.gameObject.AddComponent<StandaloneInputModule>();
            rootList = new List<UIRoot>()
            {
                new UIRoot(UILayer.Background),
                new UIRoot(UILayer.Middle),
                new UIRoot(UILayer.Popup),
                new UIRoot(UILayer.Notification),
            };
        }

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T Active<T>(params object[] args) where T : UIBase
        {
            return (T)Active(typeof(T), args);
        }

        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <param name="type"></param>
        public UIBase Active(Type type, params object[] args)
        {
            if (type is null || type.IsInterface || type.IsAbstract || type.IsSubclassOf(typeof(UIBase)) is false)
            {
                Debug.LogError("创建UI失败");
                return default;
            }

            UIOptions options = type.GetCustomAttribute<UIOptions>();
            if (options is null)
            {
                Debug.LogError("没找到UIOptions:" + type.Name);
                return default;
            }

            UIRoot root = rootList.Find(x => x.Contains(type));
            if (root is null)
            {
                root = rootList.Find(x => x.layer == options.layer);
            }

            if (root is null)
            {
                rootList.Add(root = new UIRoot(options.layer));
            }

            if (options.sceneType == SceneType.Overlap)
            {
                rootList.ForEach(x => x.Disable());
            }

            return root.Active(options, type, args);
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

            root.Inactive(type);
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

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}
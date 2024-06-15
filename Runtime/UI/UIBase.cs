using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Sound;
using ZGame.Resource;

namespace ZGame.UI
{
    public interface IUIFrom : IReference
    {
        string name { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
        RectTransform rect_transform { get; }

        void Awake();
        void Start(params object[] args);
        void Disable();
        void Enable();
    }

    public interface IUITitle : IUIFrom
    {
        void SetTitle(string title);
    }

    /// <summary>
    /// UI界面
    /// </summary>
    public class UIBase : IUIFrom
    {
        private GameObject _gameObject;
        private RectTransform _rectTransform;
        public string name => _gameObject.name;
        public Transform transform => _gameObject.transform;
        public GameObject gameObject => _gameObject;
        public RectTransform rect_transform => _rectTransform;


        /// <summary>
        /// 激活界面
        /// </summary>
        public virtual void Awake()
        {
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <param name="args"></param>
        public virtual void Start(params object[] args)
        {
        }

        /// <summary>
        /// 显示界面
        /// </summary>
        public virtual void Enable()
        {
            if (gameObject == null)
            {
                return;
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏界面
        /// </summary>
        public virtual void Disable()
        {
            if (gameObject == null)
            {
                return;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 释放UI界面
        /// </summary>
        public virtual void Release()
        {
            GameObject.DestroyImmediate(_gameObject);
            _gameObject = null;
            _rectTransform = null;
        }

        internal static UIBase Create(string path, Type type)
        {
            ResObject resObject = AppCore.Resource.LoadAsset<GameObject>(path);
            if (resObject.IsSuccess() is false)
            {
                AppCore.Logger.Log("加载资源失败：" + path);
                return default;
            }

            UIBase uiBase = (UIBase)RefPooled.Alloc(type);
            uiBase._gameObject = resObject.Instantiate();
            uiBase._rectTransform = uiBase._gameObject.GetComponent<RectTransform>();
            return uiBase;
        }

        public static T Create<T>(GameObject gameObject, Transform parent, params object[] args) where T : UIBase
        {
            T uiBase = RefPooled.Alloc<T>();
            uiBase._gameObject = gameObject;
            if (parent != null)
            {
                uiBase._gameObject.SetParent(parent, Vector3.zero, Vector3.zero, Vector3.one);
            }

            uiBase.Awake();
            uiBase.Start(args);
            uiBase.Enable();
            return uiBase;
        }
    }
}
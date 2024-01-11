using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    /// <summary>
    /// UI界面
    /// </summary>
    public class UIBase : IDisposable
    {
        public string name { get; }
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public RectTransform rect_transform { get; }

        public UIBase(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.name = gameObject.name;
            this.transform = gameObject.transform;
            this.rect_transform = gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// 激活界面
        /// </summary>
        public virtual void Awake()
        {
        }

        /// <summary>
        /// 显示界面
        /// </summary>
        public virtual void Enable(params object[] args)
        {
        }

        /// <summary>
        /// 隐藏界面
        /// </summary>
        public virtual void Disable()
        {
        }

        /// <summary>
        /// 释放UI界面
        /// </summary>
        public virtual void Dispose()
        {
        }


        public void StartCountDown(int count, float interval)
        {
        }

        public void StopCountDown()
        {
        }
    }
}
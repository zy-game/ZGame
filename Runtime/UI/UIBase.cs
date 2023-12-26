using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    /// <summary>
    /// UI界面
    /// </summary>
    public interface UIBase : IDisposable
    {
        string name { get; }
        GameObject gameObject { get; }


        /// <summary>
        /// 激活界面
        /// </summary>
        void Awake();

        /// <summary>
        /// 现实2界面
        /// </summary>
        void Enable();

        /// <summary>
        /// 隐藏界面
        /// </summary>
        void Disable();

        /// <summary>
        /// 释放UI界面
        /// </summary>
        void Dispose();
    }
}
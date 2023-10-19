using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public interface IUIComponentBindPipeline : IBindPipeline
    {
        string path { get; }
        UIWindow window { get; }
        GameObject gameObject { get; }
        void Enable();
        void Disable();
    }
}
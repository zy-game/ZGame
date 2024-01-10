using System;
using UnityEngine;

namespace ZGame.Window
{
    public interface UIForm : IDisposable
    {
        string name { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
        RectTransform rect_transform { get; }
        void Awake(params object[] args);
        void Enable();
        void Disable();
    }
}
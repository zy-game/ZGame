using System;
using UnityEngine;

namespace ZEngine.Window
{
    public interface IUIDropdownBindPipeline : IUIComponentBindPipeline
    {
        void AddItem(IUIDropdownTemplate template);
        void RemoveItem(string title);
        void RemoveItem(Sprite icon);

        public static IUIDropdownBindPipeline Create(UIWindow window, string path)
        {
            return default;
        }
    }

    public interface IUIDropdownTemplate : IUIComponentBindPipeline, ICloneable
    {
        Sprite icon { get; }
        string title { get; }

        void SetTitle(string title);
        void SetIcon(Sprite icon);

        public static IUIDropdownTemplate Create(UIWindow window, string path)
        {
            return default;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Runtime.Language;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Window
{
    /// <summary>
    /// UI界面本地化配置
    /// </summary>
    public interface IUIBindOptions : IOptions
    {
        void Initialize(UIWindow window);
        public static IUIBindOptions Create(string path)
        {
            return default;
        }
    }
}
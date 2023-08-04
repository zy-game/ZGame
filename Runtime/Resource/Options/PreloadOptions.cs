using System;
using UnityEngine;

namespace ZEngine.Options
{
    [Serializable]
    public sealed class PreloadOptions
    {
        [Header("是否启用")] public Switch state;
        [Header("模块名称")] public string moduleName;
    }
}
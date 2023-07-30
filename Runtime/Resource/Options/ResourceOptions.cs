using System;
using UnityEngine;

namespace ZEngine.Resource
{
    [Serializable]
    public class ResourceOptions : ScriptObject<ResourceOptions>
    {
        [Header("资源缓存刷新间隔时间")] public float refershIntervalTime = 60f;
    }
}
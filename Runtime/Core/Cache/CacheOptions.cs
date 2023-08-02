using System;
using UnityEngine;

namespace ZEngine
{
    [ConfigOptions(ConfigOptions.Localtion.Internal)]
    public sealed class CacheOptions : SingleScript<CacheOptions>
    {
        [Header("缓存时间")] public float time;
    }
}
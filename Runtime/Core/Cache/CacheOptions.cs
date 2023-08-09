using System;
using UnityEngine;

namespace ZEngine
{
    [Config(Localtion.Internal)]
    public sealed class CacheOptions : SingleScript<CacheOptions>
    {
        [Header("缓存时间")] public float time;
    }
}
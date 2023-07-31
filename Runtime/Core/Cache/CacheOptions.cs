using System;
using UnityEngine;

namespace ZEngine
{
    [ConfigOptions(ConfigOptions.Localtion.Internal)]
    public sealed class CacheOptions : ScriptObjectSingle<CacheOptions>
    {
        [Header("缓存时间")] public float time;
    }
}
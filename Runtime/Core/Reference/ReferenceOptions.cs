using System;
using UnityEngine;

namespace ZEngine
{
    [Config(Localtion.Internal)]
    public class ReferenceOptions : SingleScript<ReferenceOptions>
    {
        [Header("默认大小"), Range(1000, 1024 * 1024)]
        public uint DefaultCount = 1000;

        [Header("单个引用类型最大缓存数量"), Range(1000, 1024 * 1024)]
        public uint MaxCount = 1000;
    }
}
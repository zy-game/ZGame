using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    [ConfigOptions(ConfigOptions.Localtion.Project)]
    public class ResourceOptions : SingleScript<ResourceOptions>
    {
        [Header("模块列表")] public List<Object> modules;
    }
}
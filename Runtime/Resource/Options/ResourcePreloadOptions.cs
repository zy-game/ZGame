using System;
using UnityEngine;

namespace ZEngine.Options
{
    [ConfigOptions(ConfigOptions.Localtion.Internal)]
    public sealed class ResourcePreloadOptions : ScriptObjectSingle<ResourcePreloadOptions>
    {
        [Header("模块名称")] public string moduleName;
        [Header("模块版本")] public VersionOptions version;
    }
}
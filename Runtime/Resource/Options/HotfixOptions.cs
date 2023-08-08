using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ZEngine.Options;

namespace ZEngine.Resource
{
    [ConfigOptions(ConfigOptions.Localtion.Internal)]
    public class HotfixOptions : SingleScript<HotfixOptions>
    {
        [Header("编辑器启用热更")] public Switch useHotfix;
        [Header("编辑器加载热更脚本")] public Switch useScript;
        [Header("编辑器加载热更资源")] public Switch useAsset;
        [Header("自动加载资源包")] public Switch autoLoad;
        [Header("资源地址")] public List<URLOptions> address;
        [Header("预加载模块")] public List<PreloadOptions> preloads;
    }
}
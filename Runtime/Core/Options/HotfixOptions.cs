using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZEngine
{
    [Config(Localtion.Internal)]
    public class HotfixOptions : SingleScript<HotfixOptions>
    {
        [Header("编辑器启用热更")] public Switch useHotfix;
        [Header("编辑器加载热更脚本")] public Switch useScript;
        [Header("编辑器加载热更资源")] public Switch useAsset;
        [Header("缓存时间"), Range(60, 60 * 60)] public float cachetime;
        [Header("资源地址")] public List<URLOptions> address;
        [Header("预加载模块")] public List<ModuleOptions> preloads;
    }

    [Serializable]
    public class ModuleOptions
    {
        public Switch isOn;
        public string moduleName;
        [NonSerialized] public URLOptions url;
    }

    [Serializable]
    public sealed class URLOptions
    {
        [Header("是否启用")] public Switch state;
        [Header("别称")] public string name;
        [Header("地址")] public string address;
    }
}
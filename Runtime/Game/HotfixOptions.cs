using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZEngine
{
    [InternalConfig]
    public class HotfixOptions : ConfigScriptableObject<HotfixOptions>
    {
        [Header("编辑器启用热更")] public Switch useHotfix;
        [Header("编辑器加载热更脚本")] public Switch useScript;
        [Header("编辑器加载热更资源")] public Switch useAsset;
        [Header("游戏入口配置")] public List<GameEntryOptions> entryList;
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
    public class GameEntryOptions
    {
        [Header("是否启用")] public Switch isOn;
        [Header("代码文件名")] public string dllName;
        [Header("元数据列表")] public List<string> aotList;
        [Header("启动参数")] public List<string> paramsList;
        [Header("入口名")] public string methodName;
    }

    [Serializable]
    public sealed class URLOptions
    {
        [Header("是否启用")] public Switch state;
        [Header("别称")] public string name;
        [Header("地址")] public string address;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    [ConfigOptions(ConfigOptions.Localtion.Project)]
    public class ResourceOptions : ScriptObjectSingle<ResourceOptions>
    {
        [Header("模块列表")] public List<Object> modules;
    }

    public enum OSSService : byte
    {
        OSS,
        COS
    }

    [ConfigOptions(ConfigOptions.Localtion.Internal)]
    public class ResourceHotfixOptions : ScriptObjectSingle<ResourceHotfixOptions>
    {
        [Header("编辑器启用热更")] public Switch useHotfixInEditor;
        [Header("编辑器加载热更脚本")] public Switch useHotfixScriptInEditor;
        [Header("编辑器加载热更资源")] public Switch useHotfixResourceInEditor;
        [Header("存储服务商")] public OSSService service;
        [Header("资源地址")] public List<URLOptions> address;
    }
}
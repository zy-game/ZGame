using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Resource
{
    [ConfigOptions(ConfigOptions.Localtion.Internal)]
    public class ResourceHotfixOptions : SingleScript<ResourceHotfixOptions>
    {
        [Header("编辑器启用热更")] public Switch useHotfixInEditor;
        [Header("编辑器加载热更脚本")] public Switch useHotfixScriptInEditor;
        [Header("编辑器加载热更资源")] public Switch useHotfixResourceInEditor;
        [Header("存储服务商")] public OSSService service;
        [Header("资源地址")] public List<URLOptions> address;
    }
}
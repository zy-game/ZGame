using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ZEngine.Options;

namespace ZEngine.Resource
{
    [Config(Localtion.Internal)]
    public class HotfixOptions : SingleScript<HotfixOptions>
    {
        [Header("编辑器启用热更")] public Switch useHotfix;
        [Header("编辑器加载热更脚本")] public Switch useScript;
        [Header("编辑器加载热更资源")] public Switch useAsset;
        [Header("缓存时间"), Range(60, 60 * 60)] public float cachetime;
        [Header("资源地址")] public List<URLOptions> address;
        [Header("预加载模块")] public List<PreloadOptions> preloads;

        public UpdateOptions[] GetPreloadOptions()
        {
            List<UpdateOptions> optionsList = new List<UpdateOptions>();
            URLOptions urlOptions = HotfixOptions.instance.address.Find(x => x.state == Switch.On);
            foreach (var VARIABLE in HotfixOptions.instance.preloads)
            {
                optionsList.Add(UpdateOptions.Create(VARIABLE.moduleName, urlOptions));
            }

            return optionsList.ToArray();
        }
    }
}
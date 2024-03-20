using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace ZGame.Editor
{
    [ResourceReference("Assets/Settings/Language.asset")]
    public class LanguageConfig : BaseConfig<LanguageConfig>
    {
        public List<LanguageOptions> lanList;

        public override void OnAwake()
        {
            if (lanList is null || lanList.Count == 0)
            {
                lanList = new List<LanguageOptions>()
                {
                    new LanguageOptions() { name = "中文", filter = "zh" },
                    new LanguageOptions() { name = "英文", filter = "en" },
                };
            }
        }
    }


    [Serializable]
    public class LanguageOptions
    {
        public string name;
        public string filter;
    }
}
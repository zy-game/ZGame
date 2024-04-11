using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace ZGame.Language
{
    [RefPath("Assets/Settings/Language.asset")]
    public class LanguageConfig : BaseConfig<LanguageConfig>
    {
        [Title("语言设置"), LabelText("多语言列表"), TableList]
        public List<LanguageOptions> lanList = new()
        {
            new LanguageOptions() { name = "中文", filter = "zh" },
            new LanguageOptions() { name = "英文", filter = "en" },
        };
    }


    [Serializable]
    public class LanguageOptions
    {
        public string name;
        public string filter;
    }
}
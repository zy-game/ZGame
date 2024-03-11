using System;
using System.Collections.Generic;

namespace ZGame.Editor
{
    [ResourceReference("Assets/Settings/Language.asset")]
    public class LanguageConfig : SingletonScriptableObject<LanguageConfig>
    {
        public List<LanguageTemplete> languageTempletes;

        public override void OnAwake()
        {
            if (languageTempletes is null || languageTempletes.Count == 0)
            {
                languageTempletes = new List<LanguageTemplete>()
                {
                    new LanguageTemplete() { name = "中文", filter = "zh" },
                    new LanguageTemplete() { name = "英文", filter = "en" },
                };
            }
        }
    }


    [Serializable]
    public class LanguageTemplete
    {
        public string name;
        public string filter;
    }
}
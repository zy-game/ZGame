using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Language;

namespace ZGame.Config
{
    [CreateAssetMenu(menuName = "ZGame/Config/LanguageConfig", fileName = "LanguageConfig.asset", order = 1)]
    [RefPath("Assets/Bundles/Share/Config/LanguageConfig.asset")]
    public class LanguageConfig : Singleton<LanguageConfig>
    {
        [System.Serializable]
        public class Data
        {
            public int id;
            public string en;
            public string zh;
        }

        public override IList Config => config;
        public List<Data> config = new List<Data>();

        public string Query(int id)
        {
            return config.Find(x => x.id == id)?.zh;
        }
    }
}
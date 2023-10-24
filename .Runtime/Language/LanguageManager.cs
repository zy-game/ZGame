using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ZEngine;
using ZEngine.Network;
using ZEngine.Resource;

namespace ZEngine.Language
{
    class LanguageManager : Singleton<LanguageManager>
    {
        class LanguageData
        {
            public int id;
            public LanguageType type;
            public string info;
        }

        private Dictionary<int, ILanguageOptions> languageOptionsMap = new Dictionary<int, ILanguageOptions>();

        public ILanguageOptions Switch(int id)
        {
            if (languageOptionsMap.TryGetValue(id, out ILanguageOptions options))
            {
                return options;
            }

            return default;
        }

        public async UniTask SwitchLanguage(LanguageDefine define)
        {
            IWebRequestResult<List<LanguageData>> webRequestResult = await ZGame.Network.Get<List<LanguageData>>("");
            if (webRequestResult.result is null)
            {
                return;
            }

            foreach (var VARIABLE in webRequestResult.result)
            {
                if (languageOptionsMap.ContainsKey(VARIABLE.id))
                {
                    languageOptionsMap[VARIABLE.id].Reset(VARIABLE.info, VARIABLE.type);
                    continue;
                }

                languageOptionsMap.Add(VARIABLE.id, ILanguageOptions.Create(VARIABLE.id, VARIABLE.info, VARIABLE.type));
            }
        }
    }
}
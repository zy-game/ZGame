using System.Collections.Generic;
using ZGame.Networking;
using ZGame.Resource;

namespace ZGame.Localization
{
    public sealed class LanguageManager : SingletonBehaviour<LanguageManager>
    {
        private Dictionary<int, LanguageOptions> _optionsMap = new Dictionary<int, LanguageOptions>();

        public string GetLanguage(int guid)
        {
            if (_optionsMap.TryGetValue(guid, out LanguageOptions options) is false)
            {
                return guid.ToString();
            }

            return options.value;
        }

        public async void SwitchLanguage(Language language)
        {
            string url = GameSeting.GetNetworkResourceUrl("language_" + language + ".ini");
            List<LanguageOptions> languageOptionsList = await NetworkRequest.Get<List<LanguageOptions>>(url);
            if (languageOptionsList is null)
            {
                return;
            }

            _optionsMap.Clear();
            foreach (var VARIABLE in languageOptionsList)
            {
                _optionsMap.Add(VARIABLE.id, VARIABLE);
            }
        }

        public void Dispose()
        {
            foreach (var VARIABLE in _optionsMap.Values)
            {
                VARIABLE.Dispose();
            }

            _optionsMap.Clear();
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Networking;

namespace ZGame.Config
{
    public enum LanguageDefine : byte
    {
        English,
        Chinese,
    }

    public class Language : Singleton<Language>
    {
        private Dictionary<int, string> _language = new();

        public async void SwitchLanguage(LanguageDefine languageDefine)
        {
            _language = await NetworkRequest.Get<Dictionary<int, string>>(GameSeting.GetNetworkResourceUrl($"language_{languageDefine}.ini"));
            if (_language is null)
            {
                _language = new Dictionary<int, string>();
            }
        }

        public void BindLanguage(Transform transform, int key)
        {
            if (_language.TryGetValue(key, out string value) is false)
            {
                transform.GetComponent<Text>().text = "Error";
                Debug.LogError($"Language key:{key} not found");
                return;
            }

            LanguageHandle handle = transform.GetComponent<LanguageHandle>();
            if (handle is null)
            {
                handle = transform.gameObject.AddComponent<LanguageHandle>();
            }

            handle.Setup(key);
        }

        class LanguageHandle : MonoBehaviour
        {
            public void Setup(int key)
            {
            }
        }
    }
}
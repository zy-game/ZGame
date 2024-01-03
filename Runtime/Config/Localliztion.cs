using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Game;
using ZGame.Networking;
using ZGame.Resource;

namespace ZGame.Config
{
    public enum LanguageDefine : byte
    {
        English,
        Chinese,
    }

    public sealed class Localliztion : SingletonScriptableObject<Localliztion>
    {
        class LanguageTemplate
        {
            public string suffix;
            public LanguageDefine define;
            public Dictionary<int, string> map;
        }

        private LanguageTemplate current;
        private List<LanguageTemplate> languages;

        public override void OnAwake()
        {
            languages = new List<LanguageTemplate>();
        }

        protected override void OnSaved()
        {
        }

        public static async UniTask Setup(string url)
        {
            TextAsset asset = default;
            if (url.StartsWith("http"))
            {
                string data = await NetworkManager.Get<string>(url);
                asset = new TextAsset(data);
            }
            else
            {
                ResHandle languageHandle = ResourceManager.instance.LoadAsset(url);
                asset = languageHandle.Get<TextAsset>(GameManager.DefaultWorld.main.gameObject);
            }

            Setup(asset);
        }

        public static void Setup(TextAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            List<JObject> list = JsonConvert.DeserializeObject<List<JObject>>(asset.text);
            if (list is null || list.Count == 0)
            {
                return;
            }

            int id_num;
            string language_ch;
            string language_en;
            List<LanguageTemplate> languages = new List<LanguageTemplate>()
            {
                new LanguageTemplate() { define = LanguageDefine.Chinese, map = new Dictionary<int, string>(), suffix = "ch" },
                new LanguageTemplate() { define = LanguageDefine.English, map = new Dictionary<int, string>(), suffix = "en" },
            };
            foreach (var VARIABLE in list)
            {
                id_num = VARIABLE.Value<int>("id_num");
                language_ch = VARIABLE.Value<string>("language_ch");
                language_en = VARIABLE.Value<string>("language_en");
                languages[0].map.Add(id_num, language_ch);
                languages[1].map.Add(id_num, language_en);
            }

            instance.languages = languages;
        }

        public static void Switch(LanguageDefine define)
        {
            if (instance is null)
            {
                return;
            }

            if (instance.languages is null)
            {
                return;
            }

            instance.current = instance.languages.Find(x => x.define == define);
            //todo 这里还要处理界面上绑定的多语言
        }

        public static List<string> GetValues()
        {
            if (instance is null)
            {
                return default;
            }

            if (instance.current is null || instance.current.map == null)
            {
                return default;
            }

            return instance.current.map.Values.ToList();
        }

        public static string Get(int key)
        {
            if (instance is null)
            {
                return default;
            }

            if (instance.current is null || instance.current.map == null)
            {
                return default;
            }

            if (instance.current.map.TryGetValue(key, out string value))
            {
                return value;
            }

            return "Not Find";
        }

        public static int GetKey(string s)
        {
            if (instance is null)
            {
                return default;
            }

            if (instance.current is null || instance.current.map == null)
            {
                return default;
            }

            if (instance.current.map.ContainsValue(s))
            {
                return instance.current.map.First(x => x.Value == s).Key;
            }

            return -1;
        }
    }
}
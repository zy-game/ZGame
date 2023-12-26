using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Networking;
using ZGame.Resource;

namespace ZGame.Config
{
    public enum LanguageDefine : byte
    {
        English,
        Chinese,
    }

    public class LanguageReference
    {
        public int key;
        public Transform transform;

        internal LanguageReference(int key, Transform transform)
        {
            this.key = key;
            this.transform = transform;
        }
    }

    [Serializable]
    public class LanguageItem
    {
        public int key;
        public string value;
    }

    [Serializable]
    public class LanguageDataList
    {
        public LanguageDefine define;
        public List<LanguageItem> items;
    }

    [ResourceReference("Assets/SuprePet/Config/Language.asset")]
    public class Language : SingletonScriptableObject<Language>
    {
        public List<LanguageDataList> _languages = new();
        private List<LanguageReference> references = new();
        private LanguageDataList _currentLanguage;

        public override void OnAwake()
        {
            SwitchLanguage(GlobalConfig.instance.language);
        }

        /// <summary>
        /// 切换多语言
        /// </summary>
        /// <param name="languageDefine"></param>
        public async void SwitchLanguage(LanguageDefine languageDefine)
        {
            if (_currentLanguage is not null && _currentLanguage.define == languageDefine)
            {
                return;
            }

            _currentLanguage = _languages.Find(x => x.define == languageDefine);
            Refresh();
        }

        /// <summary>
        /// 设置多语言绑定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="transform"></param>
        public void Setup(int key, Transform transform)
        {
            if (references.Exists(x => x.transform.Equals(transform)))
            {
                return;
            }

            references.Add(new LanguageReference(key, transform));
        }

        private void SetupLanguage(int key, Transform transform)
        {
            string text = FindByKey(key);
            if (text.EndsWith(".png") is false)
            {
                TMP_Text tmpText = transform.GetComponent<TMP_Text>();
                if (tmpText != null)
                {
                    tmpText.text = text;
                    return;
                }

                Text textComponent = transform.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = text;
                }

                return;
            }

            ResHandle handle = ResourceManager.instance.LoadAsset(text);
            if (handle.EnsureLoadSuccess() is false)
            {
                return;
            }

            Image image = transform.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = handle.Get<Sprite>(transform.gameObject);
                return;
            }

            RawImage rawImage = transform.GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = handle.Get<Texture2D>(transform.gameObject);
            }
        }

        /// <summary>
        /// 刷新所有绑定的多语言组件
        /// </summary>
        public void Refresh()
        {
            for (int i = references.Count - 1; i >= 0; i--)
            {
                if (references[i].transform == null)
                {
                    references.Remove(references[i]);
                    continue;
                }

                SetupLanguage(references[i].key, references[i].transform);
            }
        }

        /// <summary>
        /// 通过key获取多语言文本
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string FindByKey(int key)
        {
            if (_currentLanguage is null)
            {
                SwitchLanguage(GlobalConfig.instance.language);
            }

            if (_currentLanguage.items is null || _currentLanguage.items.Count == 0)
            {
                return key.ToString();
            }

            LanguageItem item = _currentLanguage.items.Find(x => x.key == key);
            if (item is null)
            {
                return "Not Language";
            }

            return item.value;
        }
    }
}
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Config;
using ZGame.Language;

namespace ZGame.UI
{
    public class LanguageComponent : MonoBehaviour
    {
        public int languageCode;

        private void Awake()
        {
            AppCore.Events.Subscribe<SwitchLanguageEventArgs>(OnHandleSwitchLanguageType);
            OnRefreshLanguage();
        }

        private void OnRefreshLanguage()
        {
            if (this.TryGetComponent<TMP_Text>(out var tmpText))
            {
                tmpText.SetText(AppCore.Language.Query(languageCode));
            }
            else if (this.TryGetComponent<Text>(out var text))
            {
                text.text = AppCore.Language.Query(languageCode);
            }
        }

        private void OnDestroy()
        {
            AppCore.Events.Unsubscribe<SwitchLanguageEventArgs>(OnHandleSwitchLanguageType);
        }

        private void OnHandleSwitchLanguageType(SwitchLanguageEventArgs args)
        {
            OnRefreshLanguage();
        }
    }
}
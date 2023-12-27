using System;
using System.Collections.Generic;
using System.Linq;
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

    public interface ILocalliztion
    {
        void SwitchLanguage(LanguageDefine languageDefine);
        List<string> GetLanguageList();
        string Find(int key);
        int FindKey(string s);

        private static ILocalliztion instance;

        public static void Setup(ILocalliztion language)
        {
            instance = language;
        }

        public static void Switch(LanguageDefine define)
        {
            if (instance is null)
            {
                return;
            }

            instance.SwitchLanguage(define);
        }

        public static List<string> GetValues()
        {
            if (instance is null)
            {
                return default;
            }

            return instance.GetLanguageList();
        }

        public static string Get(int key)
        {
            if (instance is null)
            {
                return default;
            }

            return instance.Find(key);
        }

        public static int GetKey(string s)
        {
            if (instance is null)
            {
                return default;
            }

            return instance.FindKey(s);
        }
    }
}
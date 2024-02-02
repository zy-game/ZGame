using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public interface ILanguage
    {
        int id_num { get; }
        string language_ch { get; }
        string language_en { get; }
    }

    public sealed class Localliztion : Singleton<Localliztion>
    {
        private Action onSwitch;
        private IQuery queryable;
        private LanguageDefine cureent;

        private List<(string, string)> defaultMap = new()
        {
            new("是否退出？", "Are you sure to quit?"),
            new("提示", "Tips"),
            new("正在获取配置信息...", "Getting configuration information..."),
            new("资源加载失败...", "Resource loading failed..."),
            new("资源加载完成...", "Resources loaded successfully..."),
            new("正在加载资源信息...", "Loading resource information..."),
            new("资源更新完成...", "Resource update completed..."),
            new("确定", "Confirm")
        };

        /// <summary>
        /// 当前语言
        /// </summary>
        public LanguageDefine curLanguage => cureent;

        public void Setup(IQuery queryable)
        {
            this.queryable = queryable;
        }

        /// <summary>
        /// 设置多语言切换回调
        /// </summary>
        /// <param name="action"></param>
        public void SetupLanguageChangeCallback(Action action)
        {
            this.onSwitch += action;
        }

        /// <summary>
        /// 取消多语言切换回调
        /// </summary>
        /// <param name="action"></param>
        public void UnsetLanguageChangeCallback(Action action)
        {
            this.onSwitch -= action;
        }

        public void Switch(LanguageDefine language)
        {
            if (cureent == language)
            {
                return;
            }

            cureent = language;
        }

        /// <summary>
        /// 查找多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Query(int key)
        {
            ILanguage language = (ILanguage)queryable.Query(key);
            if (language is null)
            {
                return Query(key.ToString());
            }

            return cureent == LanguageDefine.English ? language.language_en : language.language_ch;
        }

        /// <summary>
        /// 查找多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Query(string key)
        {
            for (int i = 0; i < defaultMap.Count; i++)
            {
                if (defaultMap[i].Item1.Contains(key) || defaultMap[i].Item2.Contains(key))
                {
                    return cureent == LanguageDefine.English ? defaultMap[i].Item2 : defaultMap[i].Item1;
                }
            }

            return String.Empty;
        }
    }
}
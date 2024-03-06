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
using ZGame.Module;
using ZGame.Networking;
using ZGame.Resource;

namespace ZGame.Config
{
    public enum LanguageDefine : byte
    {
        English,
        Chinese,
    }


    public sealed class LanguageManager : IModule
    {
        private Action onSwitch;
        private LanguageDefine cureent;
        private List<ILanguageData> _map;

        class ILanguageData
        {
            public int id { get; }
            public string cn { get; }
            public string en { get; }

            public ILanguageData(string cn, string en) : this(-1, cn, en)
            {
            }

            public ILanguageData(int id, string cn, string en)
            {
                this.id = id;
                this.cn = cn;
                this.en = en;
            }
        }

        public void Dispose()
        {
            _map.Clear();
            _map = null;
            onSwitch = null;
            cureent = LanguageDefine.English;
            GC.SuppressFinalize(this);
        }

        public void OnAwake()
        {
            _map = new()
            {
                new(cn: "是否退出？", en: "Are you sure to quit?"),
                new(cn: "提示", en: "Tips"),
                new(cn: "正在获取配置信息...", en: "Getting configuration information..."),
                new(cn: "资源加载失败...", en: "Resource loading failed..."),
                new(cn: "资源加载完成...", en: "Resources loaded successfully..."),
                new(cn: "正在加载资源信息...", en: "Loading resource information..."),
                new(cn: "资源更新完成...", en: "Resource update completed..."),
                new(cn: "确定", en: "Confirm"),
                new(cn: "App 版本过低，请重新安装App后在使用", en: "The app version is too low. Please reinstall the app before using it")
            };
        }

        /// <summary>
        /// 当前语言
        /// </summary>
        public LanguageDefine curLanguage => cureent;

        /// <summary>
        /// 设置语言数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cn"></param>
        /// <param name="en"></param>
        public void Setup(int id, string cn, string en)
        {
            _map.Add(new ILanguageData(id, cn, en));
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

        /// <summary>
        /// 切换多语言
        /// </summary>
        /// <param name="language"></param>
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
            ILanguageData data = _map.Find(x => x.id == key);
            if (data is null)
            {
                return Query(key.ToString());
            }

            return curLanguage == LanguageDefine.Chinese ? data.cn : data.en;
        }

        /// <summary>
        /// 查找多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Query(string key)
        {
            ILanguageData data = _map.Find(x => x.cn.Contains(key) || x.en.Contains(key));
            if (data is null)
            {
                return "Not Key";
            }

            return curLanguage == LanguageDefine.Chinese ? data.cn : data.en;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Game;
using ZGame.Networking;

namespace ZGame.Language
{
    public enum CommonLanguage
    {
        Quit = -1,
        Tips = -2,
        GetConfig = -3,
        UpdateResList = -4,
        ResLoadFail = -5,
        ResUpdateFail = -6,
        ResLoadComplete = -7,
        LoadResInfo = -8,
        ResUpdateComplete = -9,
        Confirm = -10,
        Cancel = -11,
        AppLevelLow = -12,
        NotFindEntry = -13,
        LoadScene = -14,
        LoadGameFail = -15,
    }

    public sealed class LanguageManager : GameFrameworkModule
    {
        private Action onSwitch;
        private List<LanguageItem> _map;
        public string current { get; private set; }

        public override void Release()
        {
            _map.ForEach(RefPooled.Release);
            _map.Clear();
            _map = null;
            onSwitch = null;
            current = String.Empty;
        }

        public override void OnAwake(params object[] args)
        {
            _map = new List<LanguageItem>();
            AddLanguageDatable((int)CommonLanguage.Quit, cn: "是否退出？", en: "Are you sure to quit?");
            AddLanguageDatable((int)CommonLanguage.Tips, cn: "提示", en: "Tips");
            AddLanguageDatable((int)CommonLanguage.GetConfig, cn: "正在获取配置信息...", en: "Getting configuration information...");
            AddLanguageDatable((int)CommonLanguage.UpdateResList, cn: "更新资源列表中...", en: "Update Resource List...");
            AddLanguageDatable((int)CommonLanguage.ResLoadFail, cn: "资源加载失败...", en: "Resource loading failed...");
            AddLanguageDatable((int)CommonLanguage.ResUpdateFail, cn: "资源更新失败...", en: "Resource updating failed...");
            AddLanguageDatable((int)CommonLanguage.ResLoadComplete, cn: "资源加载完成...", en: "Resources loaded successfully...");
            AddLanguageDatable((int)CommonLanguage.LoadResInfo, cn: "正在加载资源信息...", en: "Loading resource information...");
            AddLanguageDatable((int)CommonLanguage.ResLoadComplete, cn: "资源更新完成...", en: "Resource update completed...");
            AddLanguageDatable((int)CommonLanguage.Confirm, cn: "确定", en: "Confirm");
            AddLanguageDatable((int)CommonLanguage.Cancel, cn: "取消", en: "Cancel");
            AddLanguageDatable((int)CommonLanguage.AppLevelLow, cn: "App 版本过低，请重新安装App后在使用", en: "The app version is too low. Please reinstall the app before using it");
            AddLanguageDatable((int)CommonLanguage.NotFindEntry, cn: "未找到入口配置...", en: "Not Find Entry Point...");
            AddLanguageDatable((int)CommonLanguage.LoadScene, cn: "加载场景中...", en: "Loading Scene...");
            AddLanguageDatable((int)CommonLanguage.LoadGameFail, cn: "加载游戏失败...", en: "Loading Game Fail...");
            Switch(GameConfig.instance.Packet.language);
        }

        /// <summary>
        /// 设置语言数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cn"></param>
        /// <param name="en"></param>
        void AddLanguageDatable(int id, string cn, string en)
        {
            LanguageItem languageItem = RefPooled.Spawner<LanguageItem>();
            languageItem.id = id;
            languageItem.SetLanguage("zh", cn);
            languageItem.SetLanguage("en", en);
            _map.Add(languageItem);
        }

        public void SetLanguage(int id, string flter, string value)
        {
            LanguageItem item = _map.FirstOrDefault(x => x.id == id);
            if (item is null)
            {
                item = RefPooled.Spawner<LanguageItem>();
                item.id = id;
                _map.Add(item);
            }

            item.SetLanguage(flter, value);
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
        /// <param name="filter"></param>
        public void Switch(string filter)
        {
            if (current == filter)
            {
                return;
            }

            current = filter;
            this.onSwitch?.Invoke();
        }

        /// <summary>
        /// 查找多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Query(int key)
        {
            LanguageItem item = _map.Find(x => x.id == key);
            if (item is null)
            {
                return "Not Key:" + key;
            }

            return item.GetLanguage(current);
        }


        public string Query(CommonLanguage value)
        {
            return Query((int)value);
        }
    }
}
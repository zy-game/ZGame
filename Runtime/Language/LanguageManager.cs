﻿using System;
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
using ZGame.Resource;

namespace ZGame.Language
{
    public sealed class LanguageManager : GameFrameworkModule
    {
        private Action onSwitch;
        private List<DefaultLanguageDefine> _map;
        public string current { get; private set; }

        public override void Release()
        {
            _map.ForEach(GameFrameworkFactory.Release);
            _map.Clear();
            _map = null;
            onSwitch = null;
            current = String.Empty;
        }

        public override void OnAwake(params object[] args)
        {
            _map = new List<DefaultLanguageDefine>();
            AddLanguageDatable(-1, cn: "是否退出？", en: "Are you sure to quit?");
            AddLanguageDatable(-1, cn: "提示", en: "Tips");
            AddLanguageDatable(-1, cn: "正在获取配置信息...", en: "Getting configuration information...");
            AddLanguageDatable(-1, cn: "更新资源列表中...", en: "Update Resource List...");
            AddLanguageDatable(-1, cn: "资源加载失败...", en: "Resource loading failed...");
            AddLanguageDatable(-1, cn: "资源加载完成...", en: "Resources loaded successfully...");
            AddLanguageDatable(-1, cn: "正在加载资源信息...", en: "Loading resource information...");
            AddLanguageDatable(-1, cn: "资源更新完成...", en: "Resource update completed...");
            AddLanguageDatable(-1, cn: "确定", en: "Confirm");
            AddLanguageDatable(-1, cn: "App 版本过低，请重新安装App后在使用", en: "The app version is too low. Please reinstall the app before using it");
            Switch(ChannelConfigList.instance.current.language);
        }

        /// <summary>
        /// 设置语言数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cn"></param>
        /// <param name="en"></param>
        public void AddLanguageDatable(int id, string cn, string en)
        {
            DefaultLanguageDefine defaultLanguageDefine = GameFrameworkFactory.Spawner<DefaultLanguageDefine>();
            defaultLanguageDefine.SetLanguage("zh", cn);
            defaultLanguageDefine.SetLanguage("en", en);
            _map.Add(defaultLanguageDefine);
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
            DefaultLanguageDefine define = _map.Find(x => x.id == key);
            if (define is null)
            {
                return Query(key.ToString());
            }

            return define.GetLanguage(current);
        }

        /// <summary>
        /// 查找多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Query(string key)
        {
            DefaultLanguageDefine define = _map.Find(x => x.id == -1 && x.Contains(key));
            if (define is null)
            {
                return "Not Key";
            }

            return define.GetLanguage(current);
        }
    }
}
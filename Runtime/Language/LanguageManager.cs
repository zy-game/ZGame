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
        class LanguageData : IDatable
        {
            public int id { get; }

            private Dictionary<string, string> map;

            public LanguageData(int id)
            {
                this.id = id;
                map = new Dictionary<string, string>();
            }

            public bool HasLanguage(string filter)
            {
                return map.ContainsKey(filter);
            }

            public void SetLanguage(string filter, string value)
            {
                if (map.ContainsKey(filter))
                {
                    map.Add(filter, value);
                }
                else
                {
                    map[filter] = value;
                }
            }

            public string GetLanguage(string filter)
            {
                if (map.ContainsKey(filter))
                {
                    return map[filter];
                }

                return "NOT FILTER:" + filter;
            }

            public bool Contains(string filter)
            {
                foreach (var VARIABLE in map.Values)
                {
                    if (VARIABLE.Contains(filter))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void Dispose()
            {
                map.Clear();
                map = null;
                GC.SuppressFinalize(this);
            }

            public bool Equals(string field, object value)
            {
                return false;
            }
        }

        private Action onSwitch;
        private List<LanguageData> _map;
        public string current { get; private set; }


        public void Dispose()
        {
            _map.ForEach(x => x.Dispose());
            _map.Clear();
            _map = null;
            onSwitch = null;
            current = String.Empty;
            GC.SuppressFinalize(this);
        }

        public override void OnAwake()
        {
            _map = new List<LanguageData>();
            Setup(-1, cn: "是否退出？", en: "Are you sure to quit?");
            Setup(-1, cn: "提示", en: "Tips");
            Setup(-1, cn: "正在获取配置信息...", en: "Getting configuration information...");
            Setup(-1, cn: "更新资源列表中...", en: "Update Resource List...");
            Setup(-1, cn: "资源加载失败...", en: "Resource loading failed...");
            Setup(-1, cn: "资源加载完成...", en: "Resources loaded successfully...");
            Setup(-1, cn: "正在加载资源信息...", en: "Loading resource information...");
            Setup(-1, cn: "资源更新完成...", en: "Resource update completed...");
            Setup(-1, cn: "确定", en: "Confirm");
            Setup(-1, cn: "App 版本过低，请重新安装App后在使用", en: "The app version is too low. Please reinstall the app before using it");
        }

        /// <summary>
        /// 设置语言数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cn"></param>
        /// <param name="en"></param>
        public void Setup(int id, string cn, string en)
        {
            LanguageData languageData = new LanguageData(id);
            languageData.SetLanguage("zh", cn);
            languageData.SetLanguage("en", en);
            _map.Add(languageData);
        }

        public void Setup<T>()
        {
            Type cfgType = typeof(T);
            MethodInfo method = cfgType.GetMethod("InitConfig", BindingFlags.Static | BindingFlags.NonPublic);
            if (method is null)
            {
                throw new NotImplementedException("InitConfig");
            }

            IList cfgList = (IList)method.Invoke(null, new object[0]);
            PropertyInfo[] fields = cfgType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo id = fields.FirstOrDefault(x => x.Name == "id");
            if (id is null)
            {
                throw new NotImplementedException("id");
            }

            for (int i = 0; i < cfgList.Count; i++)
            {
                object o = cfgList[i];
                LanguageData languageData = new LanguageData((int)id.GetValue(o));
                for (int j = 0; j < fields.Length; j++)
                {
                    if (fields[j] == id)
                    {
                        continue;
                    }

                    languageData.SetLanguage(fields[j].Name, (string)fields[j].GetValue(o));
                }

                _map.Add(languageData);
            }
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
            LanguageData data = _map.Find(x => x.id == key);
            if (data is null)
            {
                return Query(key.ToString());
            }

            return data.GetLanguage(current);
        }

        /// <summary>
        /// 查找多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Query(string key)
        {
            LanguageData data = _map.Find(x => x.id == -1 && x.Contains(key));
            if (data is null)
            {
                return "Not Key";
            }

            return data.GetLanguage(current);
        }
    }
}
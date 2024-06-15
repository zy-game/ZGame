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
using ZGame.Config;
using ZGame.Game;
using ZGame.Networking;

namespace ZGame.Language
{
    public sealed class LanguageManager : GameManager
    {
        private List<LanguageConfig.Data> config;
        public LanguageDefinition current { get; private set; }

        public override void Release()
        {
            current = LanguageDefinition.zh;
        }

        public override void OnAwake(params object[] args)
        {
            config = new();
            SetData((int)LanguageCode.Tips, cn: "提示", en: "Tips");
            SetData((int)LanguageCode.Cancel, cn: "取消", en: "Cancel");
            SetData((int)LanguageCode.Confirm, cn: "确定", en: "Confirm");
            SetData((int)LanguageCode.GameQuit, cn: "是否退出？", en: "Are you sure to quit?");
            SetData((int)LanguageCode.LoadingGameSceneInfo, cn: "加载场景中...", en: "Loading Scene...");
            SetData((int)LanguageCode.LoadingGameFailure, cn: "加载游戏失败...", en: "Loading Game Fail...");
            SetData((int)LanguageCode.NotFindAssemblyEntryPoint, cn: "未找到入口配置...", en: "Not Find Entry Point...");
            SetData((int)LanguageCode.DownloadPackageList, cn: "更新资源列表中...", en: "Update Resource List...");
            SetData((int)LanguageCode.PackageLoadingFailure, cn: "资源加载失败...", en: "Resource loading failed...");
            SetData((int)LanguageCode.DownloadPackageListFailure, cn: "资源更新失败...", en: "Resource updating failed...");
            SetData((int)LanguageCode.DownloadPackageListComplete, cn: "资源更新完成...", en: "Resource update completed...");
            SetData((int)LanguageCode.PackageLoadingComplete, cn: "资源加载完成...", en: "Resources loaded successfully...");
            SetData((int)LanguageCode.LoadingPackageBundle, cn: "正在加载资源信息...", en: "Loading resource information...");
            SetData((int)LanguageCode.DownloadManifestInfo, cn: "正在获取配置信息...", en: "Getting configuration information...");
            SetData((int)LanguageCode.ApplicationNeedUpdating, cn: "App 版本过低，请重新安装App后在使用", en: "The app version is too low. Please reinstall the app before using it");
        }

        public void SetData(LanguageConfig cfg)
        {
            config.AddRange(cfg.config);
        }

        /// <summary>
        /// 设置语言数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cn"></param>
        /// <param name="en"></param>
        void SetData(int id, string cn, string en)
        {
            config.Add(new() { id = id, en = en, zh = cn });
        }

        /// <summary>
        /// 切换多语言
        /// </summary>
        /// <param name="filter"></param>
        public void Switch(LanguageDefinition language)
        {
            if (current == language)
            {
                return;
            }

            current = language;
            SwitchLanguageEventArgs args = RefPooled.Alloc<SwitchLanguageEventArgs>();
            args.language = language;
            AppCore.Events.Dispatch(args);
        }

        /// <summary>
        /// 查找多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Query(int key)
        {
            LanguageConfig.Data languageItem = config.Find(x => x.id == key);
            if (languageItem == null)
            {
                
                return "Not Find";
            }

            switch (current)
            {
                case LanguageDefinition.en:
                    return languageItem.en;
                case LanguageDefinition.zh:
                    return languageItem.zh;
                default:
                    return languageItem.zh;
            }
        }

        /// <summary>
        /// 查找多语言
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Query(LanguageCode value)
        {
            return Query((int)value);
        }
    }
}
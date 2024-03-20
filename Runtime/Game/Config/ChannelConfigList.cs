using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Config;

namespace ZGame
{
    public class ChannelConfigList : BaseConfig<ChannelConfigList>
    {
        public string selected;
        public List<ChannelPackageOptions> pckList;

        public ChannelPackageOptions current
        {
            get { return pckList.Find(x => x.title == selected); }
        }

        public override void OnAwake()
        {
            if (pckList is null)
            {
                pckList = new List<ChannelPackageOptions>();
            }
        }

        public void Add(ChannelPackageOptions options)
        {
            pckList.Add(options);
        }

        public void Remove(ChannelPackageOptions options)
        {
            pckList.Remove(options);
        }
    }

    [Serializable]
    public class ChannelPackageOptions
    {
        /// <summary>
        /// 渠道名
        /// </summary>
        public string title;

        /// <summary>
        /// 包名
        /// </summary>
        public string packageName;

        /// <summary>
        /// 安装图标
        /// </summary>
        public Texture2D icon;

        /// <summary>
        /// 启动屏图片
        /// </summary>
        public Sprite splash;

        /// <summary>
        /// 安装名
        /// </summary>
        public string appName;

        /// <summary>
        /// 语言配置
        /// </summary>
        public string language;

        /// <summary>
        /// 启动参数
        /// </summary>
        public string args;
    }
}
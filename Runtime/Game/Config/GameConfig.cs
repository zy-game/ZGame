using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using ZGame.Language;

namespace ZGame
{
    public class GameConfig : BaseConfig<GameConfig>
    {
        /// <summary>
        /// 标题
        /// </summary>
        [LabelText("游戏名")] public string title;

        /// <summary>
        /// assembly 路径
        /// </summary>
        [FilePath, LabelText("逻辑代码路径")] public string path;

        /// <summary>
        /// 运行模式
        /// </summary>
        [LabelText("运行模式")] public CodeMode mode;

        /// <summary>
        /// APP版本
        /// </summary>
        [LabelText("APP版本")] public string version;

        /// <summary>
        /// 是否开启日志
        /// </summary>
        [LabelText("是否开启日志")] public bool isDebug;

        /// <summary>
        /// 公司名
        /// </summary>
        [LabelText("公司名")] public string companyName;

        /// <summary>
        /// 安装包地址
        /// </summary>
        [LabelText("安装包地址")] public string apkUrl;

        [ValueDropdown("GetChannelTitleList"), LabelText("当前渠道"), OnValueChanged("OnChangeChannel")]
        public string curChannel;

        [ValueDropdown("GetAddressTileList"), LabelText("服务器地址")]
        public string curAddress;

        [TableList, LabelText("服务器地址配置"), Space(10)]
        public List<IPOptions> address;

        [LabelText("渠道包配置"), Space(10)] public List<PacketOption> channels;

        public IPOptions URL
        {
            get { return address.Find(x => x.title == curAddress); }
        }

        public PacketOption Packet
        {
            get { return channels.Find(x => x.title == curChannel); }
        }

        IEnumerable GetAddressTileList()
        {
            if (address is null || address.Count == 0)
            {
                return Array.Empty<ValueDropdownItem>();
            }

            return address.Select(x => new ValueDropdownItem(x.title, x.title));
        }

        IEnumerable GetChannelTitleList()
        {
            if (channels is null || channels.Count == 0)
            {
                return Array.Empty<ValueDropdownItem>();
            }

            return channels.Select(x => new ValueDropdownItem(x.title, x.title));
        }

        private void OnChangeChannel()
        {
            Debug.Log("change");
        }
    }

    [Serializable]
    public class IPOptions
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title;

        /// <summary>
        /// 地址
        /// </summary>
        public string address;

        /// <summary>
        /// 端口
        /// </summary>
        public int port;

        public string GetUrl(string path)
        {
            if (port == 0)
            {
                return $"{address}{path}";
            }

            return $"{address}:{port}{path}";
        }
    }

    [Serializable]
    public class PacketOption
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
        [ValueDropdown("FriendlyTextureSizes")]
        public string language;

        /// <summary>
        /// 启动参数
        /// </summary>
        public string args;

        private static IEnumerable FriendlyTextureSizes()
        {
            return LanguageConfig.instance.lanList.Select(x => new ValueDropdownItem(x.name, x.filter));
        }
    }
}
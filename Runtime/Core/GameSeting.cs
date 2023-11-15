using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Localization;

namespace ZGame
{
    public sealed class Entry : Attribute
    {
    }

    [Serializable]
    public sealed class GameSeting
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool active;

        /// <summary>
        /// 是否在编辑器使用热更模式
        /// </summary>
        public bool useHotfix;

        /// <summary>
        /// 默认语言
        /// </summary>
        public Language Language;

        /// <summary>
        /// 资源配置
        /// </summary>
        public string address;

        /// <summary>
        /// 默认资源模块
        /// </summary>
        public string module;

        /// <summary>
        /// DLL 名称
        /// </summary>
        public string dll;

        /// <summary>
        /// 补元数据列表
        /// </summary>
        public List<string> aot;
    }
}
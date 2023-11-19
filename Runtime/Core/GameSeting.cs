using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Localization;
using ZGame.Resource;

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
        /// 资源模式
        /// </summary>
        public ResourceMode resMode;

        /// <summary>
        /// 运行方式
        /// </summary>
        public RuntimeMode runtime;
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
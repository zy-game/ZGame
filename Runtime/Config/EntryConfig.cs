using System;
using System.Collections.Generic;

namespace ZGame
{
    [Serializable]
    public class EntryConfig
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool active;

        /// <summary>
        /// 云存储
        /// </summary>
        public string oss;

        /// <summary>
        /// 资源配置
        /// </summary>
        public string address;

        /// <summary>
        /// 默认资源模块
        /// </summary>
        public string module;

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
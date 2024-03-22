using System;
using System.Collections.Generic;

namespace ZGame
{
    public class GameConfig : BaseConfig<GameConfig>
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title;

        /// <summary>
        /// assembly 路径
        /// </summary>
        public string path;

        /// <summary>
        /// 运行模式
        /// </summary>
        public CodeMode mode;

        /// <summary>
        /// APP版本
        /// </summary>
        public string version;

        /// <summary>
        /// 是否开启日志
        /// </summary>
        public bool isDebug;

        /// <summary>
        /// 公司名
        /// </summary>
        public string companyName;

        /// <summary>
        /// 安装包地址
        /// </summary>
        public string apkUrl;

#if UNITY_EDITOR
        [NonSerialized] public bool isShowChannels;
        [NonSerialized] public bool isShowReferences;
        [NonSerialized] public UnityEditorInternal.AssemblyDefinitionAsset assembly;
        [NonSerialized] public List<UnityEditorInternal.AssemblyDefinitionAsset> referenceAssemblyList;
#endif
    }
}
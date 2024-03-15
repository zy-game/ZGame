using System;
using System.Collections.Generic;

namespace ZGame
{
    [Serializable]
    public class GameConfig
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title;

        /// <summary>
        /// 入口DLL名称
        /// </summary>
        public string entryName;

        /// <summary>
        /// assembly 路径
        /// </summary>
        public string path;

        /// <summary>
        /// 运行模式
        /// </summary>
        public CodeMode mode;

        /// <summary>
        /// 默认资源模块
        /// </summary>
        public string module;

        /// <summary>
        /// 引用DLL
        /// </summary>
        public List<string> references;

        /// <summary>
        /// APP版本
        /// </summary>
        public string version;

        /// <summary>
        /// 当前使用的渠道
        /// </summary>
        public string currentChannel;

        public string language
        {
            get
            {
                if (currentChannelOptions is null)
                {
                    return "zh";
                }

                return currentChannelOptions.language;
            }
        }

        public ChannelOptions currentChannelOptions
        {
            get { return channels.Find(x => x.title == currentChannel); }
        }

        /// <summary>
        /// 渠道包配置
        /// </summary>
        public List<ChannelOptions> channels;
#if UNITY_EDITOR
        [NonSerialized] public bool isShowChannels;
        [NonSerialized] public bool isShowReferences;
        [NonSerialized] public UnityEditorInternal.AssemblyDefinitionAsset assembly;
        [NonSerialized] public List<UnityEditorInternal.AssemblyDefinitionAsset> referenceAssemblyList;
#endif
    }
}
using System.Collections.Generic;
using UnityEngine.Serialization;
using ZGame.Config;
using ZGame.Resource;

namespace ZGame
{
    /// <summary>
    /// 资源模式
    /// </summary>
    public enum ResourceMode : byte
    {
        Editor,
        Simulator,
    }

    public enum CodeMode
    {
        Native,
        Hotfix,
    }

    [ResourceReference("Resources/BasicConfig.asset")]
    public sealed class BasicConfig : SingletonScriptableObject<BasicConfig>
    {
        /// <summary>
        /// 当前选择的服务器地址
        /// </summary>
        public string curAddressName;

        /// <summary>
        /// 资源模式
        /// </summary>
        public ResourceMode resMode;

        /// <summary>
        /// 卸载间隔时间
        /// </summary>
        public float resTimeout = 60f;

        /// <summary>
        /// 公司名
        /// </summary>
        public string companyName;

        /// <summary>
        /// 安装包地址
        /// </summary>
        public string apkUrl;

        /// <summary>
        /// 当前游戏地址
        /// </summary>
        public IPConfig curAddress
        {
            get { return address.Find(x => x.title == curAddressName); }
        }

        /// <summary>
        /// 游戏入口列表
        /// </summary>
        public GameConfig curGame;

        /// <summary>
        /// 地址列表
        /// </summary>
        public List<IPConfig> address;

        /// <summary>
        /// 虚拟文件系统设置
        /// </summary>
        public VFSConfig vfsConfig;

        public override void OnAwake()
        {
            vfsConfig ??= new VFSConfig();
            if (address is null || address.Count == 0)
            {
                address = new List<IPConfig>();
            }
#if UNITY_EDITOR
            if (curGame.referenceAssemblyList is null)
            {
                curGame.referenceAssemblyList = new List<UnityEditorInternal.AssemblyDefinitionAsset>();
            }
#endif


            if (curGame.channels is null)
            {
                curGame.channels = new List<ChannelOptions>();
            }

            if (curGame.references is null)
            {
                curGame.references = new List<string>();
            }
        }

        /// <summary>
        /// 获取平台名（小写）
        /// </summary>
        /// <returns></returns>
        public static string GetPlatformName()
        {
#if UNITY_ANDROID
            return "android";
#elif UNITY_IPHONE
            return "ios";
#elif UNITY_WEBGL
            return "web";
#endif
            return "windows";
        }

        public static string GetPlatformOutputPath(string output)
        {
            return $"{output}/{GetPlatformName()}";
        }


        /// <summary>
        /// 获取服务器接口
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetApiUrl(string path)
        {
            return $"{instance.curAddress.GetUrl(path)}";
        }
    }
}
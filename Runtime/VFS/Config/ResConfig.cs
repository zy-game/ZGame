namespace ZGame.Resource
{
    public class ResConfig : BaseConfig<ResConfig>
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
        public float timeout = 60f;

        /// <summary>
        /// 默认资源模块
        /// </summary>
        public string defaultPackageName;

        /// <summary>
        /// 代理服务器
        /// </summary>
        public string proxy;
    }
}
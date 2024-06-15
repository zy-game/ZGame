using System.ComponentModel;

namespace ZGame
{
    public enum BuildRuler : byte
    {
        /// <summary>
        /// 将所有文件打入一个资源包中
        /// </summary>
        Once,

        /// <summary>
        /// 以单个资源为一个包
        /// </summary>
        Asset,

        /// <summary>
        /// 按文件夹打包
        /// </summary>
        Folder,
    }

    public enum BuildType : byte
    {
        Bundle,
        Bytes
    }

    /// <summary>
    /// 资源模式
    /// </summary>
    public enum ResourceMode : byte
    {
        Editor,
        Simulator,
    }

    /// <summary>
    /// 代码模式
    /// </summary>
    public enum CodeMode
    {
        Native,
        Hotfix,
    }

    /// <summary>
    /// 状态标志
    /// </summary>
    public enum Status : byte
    {
        None,
        Success,
        Cancel,
        Fail,
    }

    /// <summary>
    /// 云存储类型
    /// </summary>
    public enum OSSType
    {
        None,
        Aliyun,
        Tencent,
        Streaming,
        URL,
    }

    public enum PlayState
    {
        None,
        Playing,
        Paused,
        Complete,
        Error,
        NotSupported
    }

    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum RunningStated : byte
    {
        None,
        Ready,
        Start,
        Pause,
        Run,
        Over,
    }

  

    /// <summary>
    /// 参数类型
    /// </summary>
    public enum ParamType
    {
        Int,
        Float,
        String,
        Bool,
        Vector2,
        Vector3,
        Vector4,
        Color,
    }

    public enum SwitchType2 : byte
    {
        Sprite,
        Text,
        GameObject,
    }

    public enum SwitchType : byte
    {
        Single,
        Multiple,
    }

    /// <summary>
    /// 界面默认层级
    /// </summary>
    public enum UILayer : byte
    {
        /// <summary>
        /// 底层
        /// </summary>
        Background = 1,

        /// <summary>
        /// 内容层
        /// </summary>
        Middle = 50,

        /// <summary>
        /// 弹窗层
        /// </summary>
        Popup = 100,

        /// <summary>
        /// 通知弹窗层
        /// </summary>
        Notification = 150,
    }

    /// <summary>
    /// 网络资源类型
    /// </summary>
    public enum StreamingAssetType : byte
    {
        Texture2D,
        Sprite,
        Audio_MPEG,
        Audio_WAV,
        Bundle,
        Text,
        Briary,
    }
}
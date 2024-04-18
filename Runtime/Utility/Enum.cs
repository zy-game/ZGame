namespace ZGame
{
    /// <summary>
    /// 资源模式
    /// </summary>
    public enum ResourceMode : byte
    {
        Editor,
        Simulator,
        Internal
    }

    public enum CodeMode
    {
        Native,
        Hotfix,
    }

    public enum Status : byte
    {
        None,
        Success,
        Fail,
        Runing,
    }

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

    public enum KeyEventType : byte
    {
        Down,
        Up,
        Press
    }

    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum GameState : byte
    {
        None,
        Ready,
        Start,
        Pause,
        Run,
        Over,
    }

    /// <summary>
    /// 帧同步消息号
    /// </summary>
    internal enum SyncOpcode : byte
    {
        /// <summary>
        /// 玩家加入
        /// </summary>
        PLAYER_JOIN = 100,

        /// <summary>
        /// 玩家离开
        /// </summary>
        PLAYER_LEAVE = 101,

        /// <summary>
        /// 玩家准备
        /// </summary>
        PLAYER_READY = 102,

        /// <summary>
        /// 游戏开始
        /// </summary>
        GAME_START = 103,

        /// <summary>
        /// 同步帧数据
        /// </summary>
        UPDATE_FRAME_DATA = 104,

        /// <summary>
        /// 游戏结束
        /// </summary>
        GAME_OVER = 105
    }


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
    /// 界面显示方式
    /// </summary>
    public enum SceneType : byte
    {
        /// <summary>
        /// 叠加窗口
        /// </summary>
        Addition,

        /// <summary>
        /// 覆盖窗口
        /// </summary>
        Overlap,
    }

    /// <summary>
    /// 缓存类型
    /// </summary>
    public enum CacheType : byte
    {
        /// <summary>
        /// 零时缓存，用完就删，不会缓存此标记的物体
        /// </summary>
        Temp,

        /// <summary>
        /// 常驻行，用完回收进缓存池中，等待下次使用
        /// </summary>
        Permanent,

        /// <summary>
        /// 自动管理，由框架根据运行时状态自动管理卸载
        /// </summary>
        Auto,
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
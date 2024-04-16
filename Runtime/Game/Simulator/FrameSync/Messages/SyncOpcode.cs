namespace ZGame.Game
{
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
}
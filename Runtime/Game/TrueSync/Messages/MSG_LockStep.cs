namespace ZGame.Game.LockStep
{
    /// <summary>
    /// 帧同步消息号
    /// </summary>
    internal enum MSG_LockStep : byte
    {
        /// <summary>
        /// 玩家加入
        /// </summary>
        CS_PLAYER_JOIN = 100,

        /// <summary>
        /// 玩家加入
        /// </summary>
        SC_PLAYER_JOIN,

        /// <summary>
        /// 房间数据
        /// </summary>
        SC_ROOM_INFO,

        /// <summary>
        /// 玩家离开
        /// </summary>
        CS_PLAYER_LEAVE,

        /// <summary>
        /// 玩家离开
        /// </summary>
        SC_PLAYER_LEVAE,

        /// <summary>
        /// 玩家准备
        /// </summary>
        CS_PLAYER_READY,

        /// <summary>
        /// 玩家准备
        /// </summary>
        SC_PLAYER_READY,

        /// <summary>
        /// 开始加载游戏
        /// </summary>
        SC_LOADGAME,

        /// <summary>
        /// 玩家游戏准备完毕，等待开始
        /// </summary>
        CS_LOADCOMPLETE,

        /// <summary>
        /// 游戏开始
        /// </summary>
        SC_GAME_START,

        /// <summary>
        /// 玩家输入
        /// </summary>
        CS_PLAYER_INPUT,

        /// <summary>
        /// 同步帧数据
        /// </summary>
        SC_FRAME,

        /// <summary>
        /// 游戏结束
        /// </summary>
        SC_GAME_OVER,

        /// <summary>
        /// 游戏结算
        /// </summary>
        SC_BALANCE,
    }
}
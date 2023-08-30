namespace ZEngine.Game
{
    /// <summary>
    /// 游戏装备组件
    /// </summary>
    public interface IGameEquipComponent : IEntityComponent
    {
        /// <summary>
        /// 装备ID
        /// </summary>
        int id { get; }

        /// <summary>
        /// 装备名称
        /// </summary>
        string name { get; }

        /// <summary>
        /// 装备等级
        /// </summary>
        uint level { get; }

        /// <summary>
        /// 装备描述
        /// </summary>
        string describe { get; }

        /// <summary>
        /// 装备价值
        /// </summary>
        uint gold { get; }

        /// <summary>
        /// 装备品质
        /// </summary>
        uint quality { get; }

        /// <summary>
        /// 装备图标路径
        /// </summary>
        string icon { get; }
    }
}
namespace ZEngine.Game
{
    /// <summary>
    /// 游戏技能组件
    /// </summary>
    public interface IGameSkillComponent : IEntityComponent
    {
        /// <summary>
        /// 技能编号
        /// </summary>
        int id { get; }

        /// <summary>
        /// 技能名称
        /// </summary>
        string name { get; }

        /// <summary>
        /// 技能冷却时间
        /// </summary>
        float cd { get; }

        /// <summary>
        /// 技能使用消耗
        /// </summary>
        int use { get; }

        /// <summary>
        /// 技能等级
        /// </summary>
        uint level { get; }

        /// <summary>
        /// 装备图标路径
        /// </summary>
        string icon { get; }
    }
}
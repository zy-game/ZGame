namespace ZEngine.Game
{
    /// <summary>
    /// 道具配置
    /// </summary>
    public interface IPropOptions : IOptions
    {
        /// <summary>
        /// 道具图标
        /// </summary>
        string icon { get; set; }

        /// <summary>
        /// 道具类型
        /// </summary>
        uint type { get; }

        /// <summary>
        /// 装备等级
        /// </summary>
        uint level { get; }

        /// <summary>
        /// 品质
        /// </summary>
        byte quality { get; }
    }

    /// <summary>
    /// 装备配置
    /// </summary>
    public interface IEquipPropOptions : IPropOptions
    {
        /// <summary>
        /// 职业
        /// </summary>
        byte work { get; }
    }

    /// <summary>
    /// 消耗品配置
    /// </summary>
    public interface ConsumeablePropOptions : IPropOptions
    {
        
    }
}
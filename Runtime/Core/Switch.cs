namespace ZEngine
{
    /// <summary>
    /// 开关状态
    /// </summary>
    public enum Switch : byte
    {
        /// <summary>
        /// 关闭
        /// </summary>
        Off,

        /// <summary>
        /// 开启
        /// </summary>
        On,
    }

    /// <summary>
    /// 执行器状态
    /// </summary>
    public enum Status : byte
    {
        /// <summary>
        /// 无状态
        /// </summary>
        None,

        /// <summary>
        /// 正在执行
        /// </summary>
        Execute,

        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 失败
        /// </summary>
        Failed,
    }

    /// <summary>
    /// 技能分类
    /// </summary>
    public enum SkillType : byte
    {
        /// <summary>
        /// 增益或负面效果
        /// </summary>
        Buffer,

        /// <summary>
        /// 主动技能
        /// </summary>
        Skill,

        /// <summary>
        /// 被动技能
        /// </summary>
        Passive
    }

    /// <summary>
    /// 释放类型
    /// </summary>
    public enum UseType : byte
    {
        /// <summary>
        /// 单体释放
        /// </summary>
        Single,

        /// <summary>
        /// 区域范围
        /// </summary>
        Area,

        /// <summary>
        /// 触发型
        /// </summary>
        Trigger
    }

    /// <summary>
    /// 角色职业
    /// </summary>
    public enum Career : byte
    {
        /// <summary>
        /// 剑客
        /// </summary>
        Swordsman,

        /// <summary>
        /// 拳师
        /// </summary>
        Boxer,

        /// <summary>
        /// 法师
        /// </summary>
        Master
    }

    public enum PlayerLavel : byte
    {
        /// <summary>
        /// 普通
        /// </summary>
        Normal,

        /// <summary>
        /// 高级
        /// </summary>
        Senior,

        /// <summary>
        /// 精英
        /// </summary>
        Elite,

        /// <summary>
        /// 特殊
        /// </summary>
        Special,

        /// <summary>
        /// 区域Boss
        /// </summary>
        AreaBoss,

        /// <summary>
        /// 世界Boss
        /// </summary>
        WorldBoss,
    }
}
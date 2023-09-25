using System;
using UnityEngine;

namespace ZEngine.Game
{
    public interface IOptions
    {
        int id { get; set; }
        string name { get; set; }
    }

    /// <summary>
    /// 角色数据接口
    /// </summary>
    public interface IPlayerOptions : IOptions
    {
        string icon { get; set; }
        string prefab { get; set; }
    }

    public interface ISkillOptions : IOptions
    {
        string icon { get; set; }
        string prefab { get; set; }
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
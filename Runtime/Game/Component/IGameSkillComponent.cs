using System;
using UnityEngine;

namespace ZEngine.Game
{
    /// <summary>
    /// 技能分类
    /// </summary>
    public enum SkillType : byte
    {
        Buffer,
        Skill,
        Passive
    }

    /// <summary>
    /// 释放类型
    /// </summary>
    public enum UseType : byte
    {
        Single,
        Range
    }

    public enum PlayerType : byte
    {
        Player,
        NPC,
        Monster,
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
        Boss,

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
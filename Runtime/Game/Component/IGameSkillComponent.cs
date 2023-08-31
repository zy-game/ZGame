using System;
using UnityEngine;

namespace ZEngine.Game
{
    [Serializable]
    public sealed class SkillOptions
    {
        /// <summary>
        /// 技能名称
        /// </summary>
        [Header("技能名称")] public string name;

        /// <summary>
        /// 技能冷却时间
        /// </summary>
        [Header("技能初始冷却时间")] public float cd;

        /// <summary>
        /// 技能使用消耗
        /// </summary>
        [Header("技能消耗")] public int use;

        /// <summary>
        /// 技能等级
        /// </summary>
        [Header("最大等级")] public uint maxlevel;

        /// <summary>
        /// 装备图标路径
        /// </summary>
        [Header("装备图标路径")] public string icon;
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
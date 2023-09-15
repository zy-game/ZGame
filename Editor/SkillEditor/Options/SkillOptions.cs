using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ZEngine.Game;

namespace ZEngine.Editor.SkillEditor
{
    [Serializable]
    public sealed class SkillOptions
    {
        /// <summary>
        /// 技能编号
        /// </summary>
        [Header("技能编号")] public int id;

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
        [Header("最大等级")] public int maxlevel;

        /// <summary>
        /// 装备图标路径
        /// </summary>
        [Header("图标路径")] public string icon;

        /// <summary>
        /// 技能类型
        /// </summary>
        [Header("技能类型")] public SkillType skillType;

        /// <summary>
        /// 释放类型
        /// </summary>
        [Header("释放类型")] public UseType useType;

        /// <summary>
        /// 技能描述
        /// </summary>
        [Header("技能描述")] public string describe;

        public string path_prefab;
        public string path_hit;
        public string path_buffer;
        public List<SkillLayerData> layerDatas;

#if UNITY_EDITOR
        [NonSerialized] public Texture2D _icon;
        [NonSerialized] public GameObject skill_effect;
        [NonSerialized] public GameObject skill_hit_effect;
        [NonSerialized] public GameObject skill_buffer_effect;
#endif
    }
}
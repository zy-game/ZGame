using System;
using System.Collections.Generic;
using UnityEngine;
using ZEngine.Game;

namespace ZEngine.Editor.PlayerEditor
{
    [PackageConfig("Assets/Test/players.json")]
    public class PlayerEditorOptions : ConfigScriptableObject<PlayerEditorOptions>
    {
        public List<PlayerOptions> players;
    }

    [Serializable]
    public class PlayerOptions
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public int id;

        /// <summary>
        /// 角色名
        /// </summary>
        public string name;

        /// <summary>
        /// 角色头像
        /// </summary>
        public string headIcon;

        /// <summary>
        /// 角色预制件路径
        /// </summary>
        public string prefab;

        /// <summary>
        /// 角色初始血量
        /// </summary>
        public int hp;

        /// <summary>
        /// 角色初始能量值
        /// </summary>
        public int mp;

        /// <summary>
        /// 初始攻击力
        /// </summary>
        public int attack;

        /// <summary>
        /// 初始移动速度
        /// </summary>
        public int moveSpeed;

        /// <summary>
        /// 初始攻击速度
        /// </summary>
        public int attackSpeed;

        /// <summary>
        /// 初始物理防御值
        /// </summary>
        public int physiceDefense;

        /// <summary>
        /// 初始魔法防御值
        /// </summary>
        public int magicDefense;

        /// <summary>
        /// 初始技能
        /// </summary>
        public List<int> skills;

        /// <summary>
        /// 角色分类
        /// </summary>
        public PlayerLavel level;

        /// <summary>
        /// 角色职业
        /// </summary>
        public Career career;

        [NonSerialized] public Sprite icon;
        [NonSerialized] public GameObject playerPrefab;
    }
}
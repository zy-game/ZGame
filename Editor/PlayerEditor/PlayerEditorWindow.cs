using System;
using System.Collections.Generic;
using Editor.SkillEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZEngine.Game;
using PopupWindow = UnityEditor.PopupWindow;

namespace ZEngine.Editor.PlayerEditor
{
    [Config(Localtion.Packaged, "Assets/Test/player.json")]
    public class PlayerEditorOptions : SingleScript<PlayerEditorOptions>
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

        [NonSerialized] public Sprite icon;
        [NonSerialized] public GameObject playerPrefab;
    }

    public class PlayerEditorWindow : EngineCustomEditor
    {
        [MenuItem("Game/Player Editor")]
        public static void Open()
        {
            GetWindow<PlayerEditorWindow>(false, "角色编辑器", true);
        }

        protected override void SaveChanged()
        {
            PlayerEditorOptions.instance.Saved();
        }

        protected override void Actived()
        {
            if (PlayerEditorOptions.instance.players is null || PlayerEditorOptions.instance.players.Count is 0)
            {
                PlayerEditorOptions.instance.players = new List<PlayerOptions>();
                return;
            }

            foreach (var VARIABLE in PlayerEditorOptions.instance.players)
            {
                AddDataItem(VARIABLE.name, VARIABLE);
            }
        }

        protected override void CreateNewItem()
        {
            PlayerOptions options = new PlayerOptions() { name = "未命名" };
            PlayerEditorOptions.instance.players.Add(options);
            AddDataItem(options.name, options);
            SaveChanged();
        }

        private SkillSelectorWindow skillSelectorWindow;

        protected override void DrawingItemDataView(object data)
        {
            PlayerOptions options = (PlayerOptions)data;
            options.id = EditorGUILayout.IntField("角色编号", options.id);
            options.name = EditorGUILayout.TextField("角色名", options.name);
            if (options.icon == null && options.headIcon.IsNullOrEmpty() is false)
            {
                options.icon = AssetDatabase.LoadAssetAtPath<Sprite>(options.headIcon);
            }

            options.icon = (Sprite)EditorGUILayout.ObjectField("角色头像", options.icon, typeof(Sprite), false);
            if (options.icon != null)
            {
                options.headIcon = AssetDatabase.GetAssetPath(options.icon);
            }

            if (options.playerPrefab == null && options.prefab.IsNullOrEmpty() is false)
            {
                options.playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(options.prefab);
            }

            options.playerPrefab = (GameObject)EditorGUILayout.ObjectField("角色预制件", options.playerPrefab, typeof(GameObject), false);
            if (options.playerPrefab != null)
            {
                options.prefab = AssetDatabase.GetAssetPath(options.playerPrefab);
            }


            options.hp = EditorGUILayout.IntField("初始血量", options.hp);
            options.mp = EditorGUILayout.IntField("初始能量值", options.mp);
            options.attack = EditorGUILayout.IntField("初始攻击力", options.attack);
            options.moveSpeed = EditorGUILayout.IntField("初始移动速度", options.moveSpeed);
            options.attackSpeed = EditorGUILayout.IntField("初始攻击速度", options.attackSpeed);
            options.physiceDefense = EditorGUILayout.IntField("初始物理防御值", options.physiceDefense);
            options.magicDefense = EditorGUILayout.IntField("初始魔法防御值", options.magicDefense);
            if (options.skills is null)
            {
                options.skills = new List<int>();
            }

            GUILayout.Label("技能列表");
            for (int i = 0; i < options.skills.Count; i++)
            {
                SkillOptions skillOptions = SkillDataList.instance.optionsList.Find(x => x.id == options.skills[i]);
                if (skillOptions is null)
                {
                    continue;
                }

                GUILayout.Label(AssetDatabase.LoadAssetAtPath<Texture2D>(skillOptions.icon), GUILayout.Width(100), GUILayout.Height(100));
            }

            GUIContent content = new GUIContent("+");
            Rect r = EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(content))
            {
                if (skillSelectorWindow is null)
                {
                    skillSelectorWindow = new SkillSelectorWindow();
                }

                skillSelectorWindow.size = new Vector2(r.width, SkillDataList.instance.optionsList.Count * 60);
                PopupWindow.Show(r, skillSelectorWindow);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    class SkillSelectorWindow : PopupWindowContent
    {
        public Vector2 size;

        public override Vector2 GetWindowSize()
        {
            return size;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            foreach (var VARIABLE in SkillDataList.instance.optionsList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(AssetDatabase.LoadAssetAtPath<Texture2D>(VARIABLE.icon), GUILayout.Width(50), GUILayout.Height(50));
                GUILayout.Label(VARIABLE.name);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}
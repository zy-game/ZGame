using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using ZEngine.Editor.SkillEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;
using ZEngine.Game;
using PopupWindow = UnityEditor.PopupWindow;

namespace ZEngine.Editor.PlayerEditor
{
    public class PlayerEditorWindow : EngineCustomEditor
    {
        // [MenuItem("工具/编辑器/角色编辑器")]
        public static void Open()
        {
            GetWindow<PlayerEditorWindow>(false, "角色编辑器", true);
        }

        private string cfgPath = String.Empty;
        private int index = 0;
        private List<Type> optionsTypes = new List<Type>();

        protected override void SaveChanged()
        {
            PlayerEditorOptions.instance.Saved();
        }

        protected override MenuListItem[] GetMenuList()
        {
            if (PlayerEditorOptions.instance.players is null || PlayerEditorOptions.instance.players.Count is 0)
            {
                PlayerEditorOptions.instance.players = new List<PlayerOptions>();
            }

            List<MenuListItem> items = new List<MenuListItem>();
            foreach (var VARIABLE in PlayerEditorOptions.instance.players)
            {
                items.Add(new MenuListItem()
                {
                    name = VARIABLE.name,
                    data = VARIABLE
                });
            }

            return items.ToArray();
        }


        protected override void Actived()
        {
            if (PlayerEditorOptions.instance.players is null || PlayerEditorOptions.instance.players.Count is 0)
            {
                PlayerEditorOptions.instance.players = new List<PlayerOptions>();
            }
        }

        protected override void CreateNewItem()
        {
            PlayerOptions playerOptions = new PlayerOptions();
            playerOptions.name = "未命名";
            playerOptions.id = 10000000 + PlayerEditorOptions.instance.players.Count;
            PlayerEditorOptions.instance.players.Add(playerOptions);
            SaveChanged();
        }

        private SkillSelectorWindow skillSelectorWindow;

        protected override void DrawingItemDataView(object data, float width)
        {
            PlayerOptions options = (PlayerOptions)data;
            options.icon = (Sprite)EditorGUILayout.ObjectField("角色头像", options.icon, typeof(Sprite), false);
            if (options.icon != null)
            {
                options.headIcon = AssetDatabase.GetAssetPath(options.icon);
            }

            options.id = EditorGUILayout.IntField("角色编号", options.id);
            options.name = EditorGUILayout.TextField("角色名", options.name);
            options.career = (Career)EditorGUILayout.EnumPopup("角色职业", options.career);
            options.level = (PlayerLavel)EditorGUILayout.EnumPopup("角色分类", options.level);

            if (options.icon == null && options.headIcon.IsNullOrEmpty() is false)
            {
                options.icon = AssetDatabase.LoadAssetAtPath<Sprite>(options.headIcon);
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
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            for (int i = options.skills.Count - 1; i >= 0; i--)
            {
                SkillOptions skillOptions = SkillDataList.instance.optionsList.Find(x => x.id == options.skills[i]);
                if (skillOptions is null)
                {
                    options.skills.Remove(options.skills[i]);
                    SaveChanged();
                    this.Repaint();
                    continue;
                }

                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                GUILayout.Label(AssetDatabase.LoadAssetAtPath<Texture2D>(skillOptions.icon), EditorStyles.helpBox, GUILayout.Width(100), GUILayout.Height(100));
                GUILayout.BeginVertical();
                GUILayout.Label(skillOptions.name);
                GUILayout.Label($"技能类型：{skillOptions.skillType.ToString()}");
                GUILayout.Label($"释放类型：{skillOptions.useType.ToString()}");
                GUILayout.Label($"技能描述：{skillOptions.describe}");
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(String.Empty, GUI_STYLE_MINUS))
                {
                    options.skills.Remove(options.skills[i]);
                    this.Repaint();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndHorizontal();
            GUIContent content = new GUIContent("+");
            Rect r = EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(content))
            {
                if (skillSelectorWindow is null)
                {
                    skillSelectorWindow = new SkillSelectorWindow();
                }

                skillSelectorWindow.options = options;
                skillSelectorWindow.playerEditorWindow = this;
                skillSelectorWindow.size = new Vector2(r.width, SkillDataList.instance.optionsList.Count * 60);

                PopupWindow.Show(r, skillSelectorWindow);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    class SkillSelectorWindow : PopupWindowContent
    {
        public Vector2 size;
        public PlayerOptions options;
        public PlayerEditorWindow playerEditorWindow;

        public override Vector2 GetWindowSize()
        {
            return size;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            foreach (var VARIABLE in SkillDataList.instance.optionsList)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(AssetDatabase.LoadAssetAtPath<Texture2D>(VARIABLE.icon), GUILayout.Width(50), GUILayout.Height(50));
                GUILayout.BeginVertical();
                GUILayout.Label(VARIABLE.name);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(String.Empty, EngineCustomEditor.GUI_STYLE_ADD_BUTTON))
                {
                    var item = options.skills.Find(x => x == VARIABLE.id);
                    if (item > 0)
                    {
                        EditorUtility.DisplayDialog("错误", "该角色已添加过相同技能", "确定");
                        this.editorWindow.Close();
                        return;
                    }

                    options.skills.Add(VARIABLE.id);
                    playerEditorWindow.SaveChanges();
                    this.editorWindow.Close();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}
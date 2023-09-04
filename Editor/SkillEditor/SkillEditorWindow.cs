using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using ZEngine;
using ZEngine.Editor;
using ZEngine.Game;

namespace Editor.SkillEditor
{
    [Config(Localtion.Packaged, "Assets/Test/skill.json")]
    public sealed class SkillDataList : SingleScript<SkillDataList>
    {
        public List<SkillOptions> optionsList;
    }

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

        public List<SkillLayerData> layerDatas;

#if UNITY_EDITOR
        [NonSerialized] public Texture2D _icon;
#endif
    }

    [Serializable]
    public sealed class SkillLayerData
    {
        public int index;
        public string name;
        public Switch state;
    }

    public class SkillEditorWindow : EngineCustomEditor
    {
        [MenuItem("Game/SkillEditor")]
        public static void Open()
        {
            GetWindow<SkillEditorWindow>(false, "技能编辑器", true);
        }

        private float layerWidth = 0;
        private float axisWidth = 0;
        private const string frameLine = "OverrideMargin"; // new GUIStyle("OverrideMargin");
        private const string seleteStyle = "MeTransitionSelect"; // new GUIStyle("MeTransitionSelect");
        private const string frameRangeStyle = "SelectionRect"; // new GUIStyle("SelectionRect");

        protected override void SaveChanged()
        {
            SkillDataList.instance.Saved();
        }

        protected override void Actived()
        {
            if (SkillDataList.instance.optionsList is null || SkillDataList.instance.optionsList.Count is 0)
            {
                SkillDataList.instance.optionsList = new List<SkillOptions>();
                return;
            }

            foreach (var VARIABLE in SkillDataList.instance.optionsList)
            {
                AddDataItem(VARIABLE.name, VARIABLE);
            }
        }

        protected override void CreateNewItem()
        {
            SkillOptions options = new SkillOptions() { name = "未命名" };
            SkillDataList.instance.optionsList.Add(options);
            AddDataItem(options.name, options);
            SaveChanged();
        }

        protected override void DrawingItemDataView(object data, float width)
        {
            SkillOptions options = (SkillOptions)data;
            layerWidth = width / 3;
            axisWidth = width - layerWidth;
            options.id = EditorGUILayout.IntField("技能编号", options.id);
            options.name = EditorGUILayout.TextField("技能名称", options.name);
            if (options._icon == null && options.icon.IsNullOrEmpty() is false)
            {
                options._icon = AssetDatabase.LoadAssetAtPath<Texture2D>(options.icon);
            }

            options._icon = (Texture2D)EditorGUILayout.ObjectField("角色头像", options._icon, typeof(Texture2D), false);
            if (options._icon != null)
            {
                options.icon = AssetDatabase.GetAssetPath(options._icon);
            }

            options.cd = EditorGUILayout.FloatField("冷却时间", options.cd);
            options.use = EditorGUILayout.IntField("技能消耗", options.use);
            options.maxlevel = EditorGUILayout.IntField("最大等级", options.maxlevel);
            options.skillType = (SkillType)EditorGUILayout.EnumPopup("技能类型", options.skillType);
            options.useType = (UseType)EditorGUILayout.EnumPopup("释放类型", options.useType);
            options.describe = EditorGUILayout.TextField("技能描述", options.describe);
            if (options.layerDatas is null)
            {
                options.layerDatas = new List<SkillLayerData>();
            }

            Rect layout = EditorGUILayout.BeginHorizontal("OL box NoExpand");
            if (GUILayout.Button("+", EditorStyles.toolbarDropDown))
            {
                options.layerDatas.Add(new SkillLayerData() { name = "Layer_" + options.layerDatas.Count, index = options.layerDatas.Count });
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("OL box NoExpand", GUILayout.Width(layerWidth));
            GUILayout.BeginHorizontal("OL box NoExpand", GUILayout.Width(layerWidth));
            DrawingPlayContorller();
            GUILayout.FlexibleSpace();
            DrawingLayerContoller();
            GUILayout.EndHorizontal();
            if (options.layerDatas.Count is 0)
            {
                GUILayout.Label("No Data");
            }
            else
            {
                foreach (var VARIABLE in options.layerDatas)
                {
                    DrawingTimeLayer(VARIABLE, layerWidth);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            Rect frameLayout = EditorGUILayout.BeginVertical("OL box NoExpand", GUILayout.Width(axisWidth));
            Rect keyRange = EditorGUILayout.BeginHorizontal("OL box NoExpand", GUILayout.Height(25));
            DrawingTimeAxisTilet(options, axisWidth);
            GUILayout.EndHorizontal();
            if (options.layerDatas.Count is 0)
            {
                GUILayout.Label("No Data");
            }
            else
            {
                foreach (var VARIABLE in options.layerDatas)
                {
                    DrawingTimeAxis(VARIABLE, axisWidth);
                }
            }

            GUILayout.FlexibleSpace();
            if (Event.current.type == EventType.MouseDown && frameLayout.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                float index = (Event.current.mousePosition.x - keyRange.x) / keyRange.width;
                Engine.Console.Log(keyRange, frameLayout, Event.current.mousePosition, index);
                Event.current.Use();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            DrawingCurrentFrameLine(layout);
        }

        private void DrawingCurrentFrameLine(Rect layout)
        {
            GUILayout.Space(-layout.height);

            GUI.Box(new Rect(layout.x + layerWidth, layout.y + layout.height, 4, position.height - layout.y - 30), "", "U2D.createRect");
        }

        private void DrawingPlayContorller()
        {
            if (GUILayout.Button("▶", EditorStyles.toolbarButton))
            {
            }

            if (GUILayout.Button("||", EditorStyles.toolbarButton))
            {
            }

            if (GUILayout.Button("←", EditorStyles.toolbarButton))
            {
            }

            if (GUILayout.Button("→", EditorStyles.toolbarButton))
            {
            }
        }

        private void DrawingLayerContoller()
        {
            if (GUILayout.Button("", "OL Plus"))
            {
            }
        }

        private void DrawingTimeAxisTilet(SkillOptions options, float width)
        {
            int skillFrameCount = 100;
            float space = (width - skillFrameCount) / (float)skillFrameCount;
            GUILayout.Space(space);
            for (int i = 0; i < 100; i++)
            {
                if (i % 5 == 0)
                {
                    GUILayout.Box("", "OverrideMargin", GUILayout.Height(15), GUILayout.Width(1));
                }
                else
                {
                    GUILayout.Box("", "OverrideMargin", GUILayout.Height(3), GUILayout.Width(1));
                }

                GUILayout.Space(space);
            }
        }

        private void DrawingTimeLayer(SkillLayerData skillLayerData, float width)
        {
            Rect layout = EditorGUILayout.BeginHorizontal("OL box NoExpand", GUILayout.Width(width), GUILayout.Height(30));
            skillLayerData.name = GUILayout.TextField(skillLayerData.name, EditorStyles.toolbarTextField);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawingTimeAxis(SkillLayerData skillLayerData, float width)
        {
            Color backup = GUI.color;
            GUI.backgroundColor = skillLayerData.index % 2 == 0 ? Color.clear : new Color(0.8f, 0.8f, 0.8f, 1f);
            Rect layout = EditorGUILayout.BeginHorizontal("MeTransitionSelect", GUILayout.Width(width), GUILayout.Height(30));
            if (Event.current.type == EventType.MouseDown && layout.Contains(Event.current.mousePosition) && Event.current.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Locked"), skillLayerData.state == Switch.On, () => { skillLayerData.state = skillLayerData.state == Switch.On ? Switch.Off : Switch.On; });
                menu.ShowAsContext();
            }

            GUILayout.Box(String.Empty, "SelectionRect", GUILayout.Height(28), GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.color = backup;
        }
    }
}
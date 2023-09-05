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
            layerWidth = Mathf.Min(width / 3, 300);
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

            layout = EditorGUILayout.BeginHorizontal(boxStyle);
            {
                if (GUILayout.Button("+", EditorStyles.toolbarDropDown))
                {
                    options.layerDatas.Add(new SkillLayerData() { name = "Layer_" + options.layerDatas.Count, index = options.layerDatas.Count });
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(boxStyle, GUILayout.Width(layerWidth));
                {
                    GUILayout.BeginHorizontal(boxStyle, GUILayout.Width(layerWidth));
                    {
                        DrawingPlayContorller();
                        GUILayout.FlexibleSpace();
                        DrawingLayerContoller();
                    }
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
                }


                frameLayout = EditorGUILayout.BeginVertical(boxStyle, GUILayout.Width(axisWidth));
                {
                    keyRange = EditorGUILayout.BeginHorizontal(boxStyle, GUILayout.Height(25));
                    {
                        DrawingTimeAxisTilet(options, axisWidth);
                    }
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
                    if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && frameLayout.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        float index = (Event.current.mousePosition.x - keyRange.x) / keyRange.width;
                        currentFrameIndex = (int)(index * frameCount);
                        Event.current.Use();
                        DrawingCurrentFrameLine(layout);
                        this.Repaint();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            DrawingCurrentFrameLine(layout);
        }

        private Rect frameLayout;
        private Rect keyRange;
        private Rect layout;
        private int frameCount = 0;
        private float frameCountSpace = 0;
        private int currentFrameIndex = 0;

        private void DrawingCurrentFrameLine(Rect layout)
        {
            float offset = layout.x + layerWidth + frameCountSpace * currentFrameIndex + currentFrameIndex;
            GUI.Box(new Rect(GetFrameIndexRect(currentFrameIndex), layout.y + layout.height + 5, 2, position.height - layout.y - 40), "", frameLine);
        }

        private float GetFrameIndexRect(int index)
        {
            return layout.x + layerWidth + frameCountSpace * index + index;
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
            frameCount = 100;
            frameCountSpace = (width - frameCount) / (float)frameCount;
            for (int i = 0; i < 100; i++)
            {
                if (i % 5 == 0)
                {
                    GUIContent line = new GUIContent(String.Empty);
                    GUILayout.Box(line, frameLine, GUILayout.Height(15), GUILayout.Width(1));
                    Rect spaceLable = GUILayoutUtility.GetLastRect();
                    GUI.Label(new Rect(spaceLable.x, spaceLable.y, 30, 20), i.ToString());
                }
                else
                {
                    GUILayout.Box("", frameLine, GUILayout.Height(3), GUILayout.Width(1));
                }

                GUILayout.Space(frameCountSpace);
            }
        }

        private void DrawingTimeLayer(SkillLayerData skillLayerData, float width)
        {
            Rect layout = EditorGUILayout.BeginHorizontal(boxStyle, GUILayout.Width(width), GUILayout.Height(30));
            skillLayerData.name = GUILayout.TextField(skillLayerData.name, EditorStyles.toolbarTextField);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawingTimeAxis(SkillLayerData skillLayerData, float width)
        {
            Rect sliderLayout = EditorGUILayout.BeginHorizontal(seleteStyle, GUILayout.Width(width), GUILayout.Height(30));
            if (Event.current.type == EventType.MouseDown && sliderLayout.Contains(Event.current.mousePosition) && Event.current.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Locked"), skillLayerData.state == Switch.On, () => { skillLayerData.state = skillLayerData.state == Switch.On ? Switch.Off : Switch.On; });
                menu.AddItem(new GUIContent("Edit State"), false, () => { });
                menu.ShowAsContext();
            }

            float offset = GetFrameIndexRect(skillLayerData.startFrameIndex);
            int count = skillLayerData.endFrameIndex - skillLayerData.startFrameIndex <= 0 ? 1 : skillLayerData.endFrameIndex - skillLayerData.startFrameIndex;
            float sliderWidth = frameCountSpace * count + 1;
            Rect sliderRect = new Rect(GetFrameIndexRect(skillLayerData.startFrameIndex), sliderLayout.y, sliderWidth, sliderLayout.height);
            GUI.Box(sliderRect, String.Empty, frameRangeStyle);
            Engine.Console.Log(skillLayerData.startFrameIndex, skillLayerData.endFrameIndex, count, sliderWidth, sliderRect);
            LeftDrag(skillLayerData, sliderRect);
            RightDrage(skillLayerData, sliderRect);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void LeftDrag(SkillLayerData skillLayerData, Rect rect)
        {
            EditorGUIUtility.AddCursorRect(new Rect(rect.x, rect.y, 2, rect.height), MouseCursor.ResizeHorizontal);
            switch (Event.current.rawType)
            {
                //开始拖拽分割线
                case EventType.MouseDown:
                    Event.current.Use();
                    //todo 判断点击的是否是左边
                    break;
                case EventType.MouseDrag:
                    float index = (Event.current.mousePosition.x - keyRange.x) / keyRange.width;
                    skillLayerData.startFrameIndex = (int)(index * frameCount);
                    if (skillLayerData.startFrameIndex < 0)
                    {
                        skillLayerData.startFrameIndex = 0;
                    }

                    if (skillLayerData.startFrameIndex > skillLayerData.endFrameIndex - 1)
                    {
                        skillLayerData.startFrameIndex = skillLayerData.endFrameIndex - 1;
                    }

                    Event.current.Use();
                    Repaint();
                    break;
                //结束拖拽分割线   
                case EventType.MouseUp:
                    SaveChanged();
                    Event.current.Use();
                    break;
            }
        }

        private void RightDrage(SkillLayerData skillLayerData, Rect rect)
        {
            EditorGUIUtility.AddCursorRect(new Rect(rect.x + rect.width - 2, rect.y, 2, rect.height), MouseCursor.ResizeHorizontal);
            switch (Event.current.rawType)
            {
                //开始拖拽分割线
                case EventType.MouseDown:
                    Event.current.Use();
                    //todo 判断点击的是否是右边
                    break;
                case EventType.MouseDrag:
                    float index = (Event.current.mousePosition.x - keyRange.x) / keyRange.width;
                    skillLayerData.endFrameIndex = (int)(index * frameCount);
                    if (skillLayerData.endFrameIndex >= frameCount - 1)
                    {
                        skillLayerData.endFrameIndex = frameCount - 1;
                    }

                    if (skillLayerData.endFrameIndex < skillLayerData.startFrameIndex)
                    {
                        skillLayerData.endFrameIndex = skillLayerData.startFrameIndex + 1;
                    }

                    Event.current.Use();
                    Repaint();
                    break;
                //结束拖拽分割线   
                case EventType.MouseUp:
                    SaveChanged();
                    Event.current.Use();
                    break;
            }
        }
    }
}
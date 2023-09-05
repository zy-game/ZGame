using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using XNode;
using XNodeEditor;
using ZEngine;
using ZEngine.Editor;
using ZEngine.Game;

namespace Editor.SkillEditor
{
    class SkillStateEditor : NodeGraph
    {
        private SkillEditorWindow skillEditorWindow;

        public SkillStateEditor(SkillEditorWindow editorWindow, Rect layout)
        {
        }
    }

    public class SkillEditorWindow : EngineCustomEditor
    {
        [MenuItem("Game/Skill Editor")]
        public static void Open()
        {
            GetWindow<SkillEditorWindow>(false, "技能编辑器", true);
        }

        private float layerWidth = 0;
        private float axisWidth = 0;
        private bool isLeft = false;
        private bool isRight = false;
        private int frameCount = 0;
        private float frameCountSpace = 0;
        private int currentFrameIndex = 0;
        private Rect frameLayout;
        private const string GUI_STYLE_FRAME_LINE = "OverrideMargin"; // new GUIStyle("OverrideMargin");
        private const string GUI_STYLE_LAYER_BACKGROUND = "MeTransitionSelect"; // new GUIStyle("MeTransitionSelect");
        private const string GUI_STYLE_LAYER_RANGE_SLIDER = "SelectionRect"; // new GUIStyle("SelectionRect");
        private CustomGraphView stateEditor;

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

            Rect layout = EditorGUILayout.BeginHorizontal(GUI_STYLE_BOX_BACKGROUND);
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
                GUILayout.BeginVertical(GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(layerWidth));
                {
                    GUILayout.BeginHorizontal(GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(layerWidth));
                    {
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


                Rect temp = EditorGUILayout.BeginVertical(GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(axisWidth));
                {
                    if (Rect.zero.Equals(temp) is false)
                    {
                        frameLayout = temp;
                    }

                    if (stateEditor is null)
                    {
                        stateEditor = new CustomGraphView(this);
                    }

                    if (stateEditor is not null)
                    {
                        stateEditor.OnGUI(frameLayout);
                    }
                    else
                    {
                        Rect keyRange = EditorGUILayout.BeginHorizontal(GUI_STYLE_BOX_BACKGROUND, GUILayout.Height(25));
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
                                DrawingTimeAxis(VARIABLE, axisWidth, keyRange);
                            }
                        }

                        if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && frameLayout.Contains(Event.current.mousePosition) && Event.current.button == 0)
                        {
                            float index = (Event.current.mousePosition.x - keyRange.x) / keyRange.width;
                            currentFrameIndex = (int)(index * frameCount);
                            Event.current.Use();
                            DrawingCurrentFrameLine(layout);
                            this.Repaint();
                        }
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            if (stateEditor is null)
            {
                DrawingCurrentFrameLine(layout);
            }
        }


        private void DrawingCurrentFrameLine(Rect layout)
        {
            float offset = layout.x + layerWidth + frameCountSpace * currentFrameIndex + currentFrameIndex;
            GUI.Box(new Rect(GetFrameIndexOffset(layout.x, currentFrameIndex), layout.y + layout.height + 5, 2, position.height - layout.y - 40), "", GUI_STYLE_FRAME_LINE);
        }

        private float GetFrameIndexOffset(float offset, int index)
        {
            return offset + layerWidth + frameCountSpace * index + index;
        }

        private void DrawingLayerContoller()
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

            GUILayout.FlexibleSpace();
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
                    GUILayout.Box(line, GUI_STYLE_FRAME_LINE, GUILayout.Height(15), GUILayout.Width(1));
                    Rect spaceLable = GUILayoutUtility.GetLastRect();
                    GUI.Label(new Rect(spaceLable.x, spaceLable.y, 30, 20), i.ToString());
                }
                else
                {
                    GUILayout.Box("", GUI_STYLE_FRAME_LINE, GUILayout.Height(3), GUILayout.Width(1));
                }

                GUILayout.Space(frameCountSpace);
            }
        }

        private void DrawingTimeLayer(SkillLayerData skillLayerData, float width)
        {
            Rect layout = EditorGUILayout.BeginHorizontal(GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(width), GUILayout.Height(30));
            skillLayerData.name = GUILayout.TextField(skillLayerData.name, EditorStyles.toolbarTextField);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawingTimeAxis(SkillLayerData skillLayerData, float width, Rect keyRect)
        {
            Rect sliderLayout = EditorGUILayout.BeginHorizontal(GUI_STYLE_LAYER_BACKGROUND, GUILayout.Width(width), GUILayout.Height(30));
            float left_offset = GetFrameIndexOffset(0, skillLayerData.startFrameIndex);
            float right_offset = GetFrameIndexOffset(0, skillLayerData.endFrameIndex);
            int count = skillLayerData.endFrameIndex - skillLayerData.startFrameIndex <= 0 ? 1 : skillLayerData.endFrameIndex - skillLayerData.startFrameIndex;
            float sliderWidth = frameCountSpace * count + 1;
            Rect boxRect = new Rect(left_offset, sliderLayout.y, right_offset - left_offset, sliderLayout.height);
            GUI.Box(boxRect, String.Empty, GUI_STYLE_LAYER_RANGE_SLIDER);
            if (Event.current.type == EventType.MouseDown && boxRect.Contains(Event.current.mousePosition) && Event.current.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Locked"), skillLayerData.state == Switch.On, () => { skillLayerData.state = skillLayerData.state == Switch.On ? Switch.Off : Switch.On; });
                menu.AddItem(new GUIContent("Edit State"), false, () =>
                {
                    if (stateEditor is not null)
                    {
                        stateEditor.Dispose();
                    }

                    stateEditor = new CustomGraphView(this);
                });
                menu.ShowAsContext();
            }

            LayerBoxSlider(skillLayerData, boxRect, keyRect);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        private void LayerBoxSlider(SkillLayerData skillLayerData, Rect rect, Rect keyRange)
        {
            Rect left_handle = new Rect(rect.x, rect.y, 2, rect.height);
            Rect right_handle = new Rect(rect.x + rect.width - 2, rect.y, 2, rect.height);
            EditorGUIUtility.AddCursorRect(left_handle, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(right_handle, MouseCursor.ResizeHorizontal);

            switch (Event.current.rawType)
            {
                //开始拖拽分割线
                case EventType.MouseDown:
                    isLeft = left_handle.Contains(Event.current.mousePosition);
                    isRight = right_handle.Contains(Event.current.mousePosition);
                    if (isRight || isLeft)
                    {
                        Event.current.Use();
                    }

                    break;
                case EventType.MouseDrag:
                    float index = (Event.current.mousePosition.x - keyRange.x) / keyRange.width;
                    if (isRight)
                    {
                        skillLayerData.endFrameIndex = (int)(index * frameCount);
                        skillLayerData.startFrameIndex = Math.Clamp(skillLayerData.startFrameIndex, skillLayerData.startFrameIndex + 1, frameCount - 1);
                    }

                    if (isLeft)
                    {
                        skillLayerData.startFrameIndex = (int)(index * frameCount);
                        skillLayerData.startFrameIndex = Math.Clamp(skillLayerData.startFrameIndex, 0, skillLayerData.endFrameIndex - 1);
                    }

                    if (isRight || isLeft)
                    {
                        Event.current.Use();
                        Repaint();
                    }


                    break;
                //结束拖拽分割线   
                case EventType.MouseUp:
                    isRight = false;
                    isLeft = false;
                    SaveChanged();
                    break;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using ZEngine;
using ZEngine.Editor;
using ZEngine.Game;

namespace ZEngine.Editor.SkillEditor
{
    public class SkillEditorWindow : EngineCustomEditor
    {
        // [MenuItem("工具/编辑器/技能编辑器")]
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
        internal SkillMachineEditor stateEditor;
        private SkillLayerData seletion;
        private const string GUI_STYLE_FRAME_LINE = "OverrideMargin"; // new GUIStyle("OverrideMargin");
        private const string GUI_STYLE_LAYER_BACKGROUND = "MeTransitionSelect"; // new GUIStyle("MeTransitionSelect");
        private const string GUI_STYLE_LAYER_RANGE_SLIDER = "MeTransitionSelect"; // new GUIStyle("SelectionRect");
        private const string GUI_STYLE_LAYER_RANGE_SLIDER_SELECTION = "OL SelectedRow";


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

            options._icon = (Texture2D)EditorGUILayout.ObjectField("技能图标", options._icon, typeof(Texture2D), false);
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

            if (options.skill_effect == null && options.path_prefab.IsNullOrEmpty() is false)
            {
                options.skill_effect = AssetDatabase.LoadAssetAtPath<GameObject>(options.path_prefab);
            }

            options.skill_effect = (GameObject)EditorGUILayout.ObjectField("技能特效", options.skill_effect, typeof(GameObject), false);
            if (options.skill_effect != null)
            {
                options.path_prefab = AssetDatabase.GetAssetPath(options.skill_effect);
            }

            if (options.skill_hit_effect == null && options.path_hit.IsNullOrEmpty() is false)
            {
                options.skill_hit_effect = AssetDatabase.LoadAssetAtPath<GameObject>(options.path_hit);
            }

            options.skill_hit_effect = (GameObject)EditorGUILayout.ObjectField("击中特效", options.skill_hit_effect, typeof(GameObject), false);
            if (options.skill_effect != null)
            {
                options.path_hit = AssetDatabase.GetAssetPath(options.skill_hit_effect);
            }

            if (options.skill_buffer_effect == null && options.path_buffer.IsNullOrEmpty() is false)
            {
                options.skill_buffer_effect = AssetDatabase.LoadAssetAtPath<GameObject>(options.path_buffer);
            }

            options.skill_buffer_effect = (GameObject)EditorGUILayout.ObjectField("Buffer特效", options.skill_buffer_effect, typeof(GameObject), false);
            if (options.skill_effect != null)
            {
                options.path_buffer = AssetDatabase.GetAssetPath(options.skill_buffer_effect);
            }

            if (options.layerDatas is null)
            {
                options.layerDatas = new List<SkillLayerData>();
            }

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
                        Rect keyRange = EditorGUILayout.BeginHorizontal(GUI_STYLE_BOX_BACKGROUND, GUILayout.Height(29));
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
                                DrawingTimeAxis(VARIABLE, axisWidth, keyRange, frameLayout);
                            }
                        }

                        if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && frameLayout.Contains(Event.current.mousePosition) && Event.current.button == 0)
                        {
                            float index = (Event.current.mousePosition.x - keyRange.x) / keyRange.width;
                            currentFrameIndex = (int)(index * frameCount);
                            Event.current.Use();
                            DrawingCurrentFrameLine(frameLayout);
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
                DrawingCurrentFrameLine(frameLayout);
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
            GUILayout.Space(5);
            if (GUILayout.Button("▶", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
            }

            if (GUILayout.Button("||", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
            }

            if (GUILayout.Button("←", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
            }

            if (GUILayout.Button("→", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", GUI_STYLE_ADD_BUTTON))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Input Checker"), false, () => { });
                menu.AddItem(new GUIContent("Hit Checker"), false, () => { });

                menu.ShowAsContext();
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

        private void DrawingTimeAxis(SkillLayerData skillLayerData, float width, Rect keyRect, Rect range)
        {
            Rect sliderLayout = EditorGUILayout.BeginHorizontal(GUI_STYLE_LAYER_BACKGROUND, GUILayout.Width(width), GUILayout.Height(28));
            float left_offset = GetFrameIndexOffset(0, skillLayerData.startFrameIndex);
            float right_offset = GetFrameIndexOffset(0, skillLayerData.endFrameIndex);
            int count = skillLayerData.endFrameIndex - skillLayerData.startFrameIndex <= 0 ? 1 : skillLayerData.endFrameIndex - skillLayerData.startFrameIndex;
            float sliderWidth = frameCountSpace * count + 1;
            Rect boxRect = new Rect(left_offset, sliderLayout.y, right_offset - left_offset, sliderLayout.height);
            GUI.Box(boxRect, String.Empty, seletion == skillLayerData ? GUI_STYLE_LAYER_RANGE_SLIDER_SELECTION : GUI_STYLE_LAYER_RANGE_SLIDER);
            if (Event.current.type == EventType.MouseDown && boxRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.button == 1)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Locked"), skillLayerData.state == Switch.On, () => { skillLayerData.state = skillLayerData.state == Switch.On ? Switch.Off : Switch.On; });
                    menu.AddItem(new GUIContent("Edit State"), false, () =>
                    {
                        VisualElement element = new VisualElement();
                        this.rootVisualElement.Add(element);
                        element.style.left = keyRect.x + 315;
                        element.style.top = range.y + 33;
                        element.style.height = range.height - 6;
                        element.style.width = keyRect.width - 5;
                        element.Add(stateEditor = new SkillMachineEditor(this, element, skillLayerData));
                    });
                    menu.ShowAsContext();
                }

                seletion = skillLayerData;
                this.Repaint();
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
                        skillLayerData.endFrameIndex = Math.Clamp(skillLayerData.endFrameIndex, skillLayerData.startFrameIndex + 1, frameCount - 1);
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
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZEngine.Editor
{
    static public class EditorWindowExtension
    {
        private static Color _color;

        public static void BeginColor(this EditorWindow window, Color color)
        {
            _color = GUI.color;
            GUI.color = color;
        }

        public static void EndColor(this EditorWindow window)
        {
            GUI.color = _color;
        }
    }

    [InitializeOnLoad]
    public static class CruToolbar
    {
        private static readonly Type kToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject sCurrentToolbar;


        static CruToolbar()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (sCurrentToolbar == null)
            {
                UnityEngine.Object[] toolbars = Resources.FindObjectsOfTypeAll(kToolbarType);
                sCurrentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (sCurrentToolbar != null)
                {
                    FieldInfo root = sCurrentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    VisualElement concreteRoot = root.GetValue(sCurrentToolbar) as VisualElement;

                    VisualElement toolbarZone = concreteRoot.Q("ToolbarZoneRightAlign");
                    VisualElement parent = new VisualElement()
                    {
                        style =
                        {
                            flexGrow = 1,
                            flexDirection = FlexDirection.Row,
                        }
                    };
                    IMGUIContainer container = new IMGUIContainer();
                    container.onGUIHandler += OnGuiBody;
                    parent.Add(container);
                    toolbarZone.Add(parent);
                }
            }
        }

        private static void OnGuiBody()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Full setup"), EditorStyles.toolbarButton))
            {
                Debug.Log("Full setup");
            }

            if (GUILayout.Button(new GUIContent("Tools"), EditorStyles.toolbarButton))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("UI编辑器"), false, () => { UIEditor.UIEditorWindow.Open(); });
                menu.AddItem(new GUIContent("行为编辑器"), false, () => { AIEditor.AIEditorWindow.Open(); });
                menu.AddItem(new GUIContent("地图编辑器"), false, () => { MapEditor.MapEditorWindow.Open(); });
                menu.AddItem(new GUIContent("物品编辑器"), false, () => { EquipEditor.EquipEditorWindow.Open(); });
                menu.AddItem(new GUIContent("角色编辑器"), false, () => { PlayerEditor.PlayerEditorWindow.Open(); });
                menu.AddItem(new GUIContent("技能编辑器"), false, () => { SkillEditor.SkillEditorWindow.Open(); });
                menu.AddItem(new GUIContent("资源编辑器"), false, () => { ResourceBuilder.GameResourceBuilder.Open(); });
                menu.ShowAsContext();
            }

            if (GUILayout.Button("Options", EditorStyles.toolbarButton))
            {
                OptionsEditorWindow.OptionsWindow.OpenSetting();
            }

            GUILayout.EndHorizontal();
        }
    }
}
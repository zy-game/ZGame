using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;
using ZEngine.Editor.PlayerEditor;

namespace ZEngine.Editor
{
    [InitializeOnLoad]
    public static class ToolbarExtension
    {
        private static readonly Type kToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject sCurrentToolbar;


        static ToolbarExtension()
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
            if (GUILayout.Button(new GUIContent("Tools"), EditorStyles.toolbarButton))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("UI编辑器"), false, () => { UIEditor.UIEditorWindow.Open(); });
                menu.AddItem(new GUIContent("行为编辑器"), false, () => { AIEditor.AIEditorWindow.Open(); });
                menu.AddItem(new GUIContent("地图编辑器"), false, () => { MapEditor.MapEditorWindow.Open(); });
                menu.AddItem(new GUIContent("物品编辑器"), false, () => { EquipEditor.EquipEditorWindow.Open(); });
                menu.AddItem(new GUIContent("角色编辑器"), false, () => { EditorWindow.GetWindow<GamePlayerEditorWindow>(false, "角色编辑器", true); });
                menu.AddItem(new GUIContent("技能编辑器"), false, () => { SkillEditor.SkillEditorWindow.Open(); });
                menu.AddItem(new GUIContent("资源编辑器"), false, () => { ResourceBuilder.GameResourceBuilder.Open(); });

                menu.AddItem(new GUIContent("Update"), false, TestUpdatePackage);
                menu.ShowAsContext();
            }

            if (GUILayout.Button("Options", EditorStyles.toolbarButton))
            {
                OptionsEditorWindow.OptionsWindow.OpenSetting();
            }

            GUILayout.EndHorizontal();
        }

        static async void TestUpdatePackage()
        {
            ListRequest request = Client.List();
            while (request.IsCompleted == false)
            {
                Debug.Log("===========");
                await Task.Delay(1000);
            }


            foreach (var VARIABLE in request.Result)
            {
                Debug.Log(VARIABLE.name + ":" + VARIABLE.version);
            }

            SearchRequest searchRequest = Client.SearchAll();
            while (searchRequest.IsCompleted == false)
            {
                Debug.Log("===========");
                await Task.Delay(1000);
            }

            foreach (var VARIABLE in searchRequest.Result)
            {
                Debug.Log(VARIABLE.name + ":" + VARIABLE.version);
            }

            Client.Add("https://gitee.com/focus-creative-games/hybridclr_unity.git");
        }
    }
}
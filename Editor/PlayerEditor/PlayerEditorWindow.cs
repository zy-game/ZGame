using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZEngine.Editor.PlayerEditor
{
    public class PlayerEditorWindow : EditorWindow
    {
        [MenuItem("Game/Player Editor")]
        public static void Open()
        {
            GetWindow<PlayerEditorWindow>(false, "Player Editor", true);
        }

        private string search;
        private Vector2 listScroll;
        private Vector2 manifestScroll;
        private PlayerOptions selection;
        private Color inColor = new Color(1f, 0.92f, 0.01f, .8f);
        private Color outColor = new Color(0, 0, 0, 0.2f);

        private void OnEnable()
        {
            if (PlayerEditorOptions.instance.players is null)
            {
                PlayerEditorOptions.instance.players = new List<PlayerOptions>();
            }
        }

        public void OnGUI()
        {
            Toolbar();
            DrawingSceneEditor();
        }

        void Toolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("+", EditorStyles.toolbarButton))
                {
                    PlayerEditorOptions.instance.players.Add(new PlayerOptions());
                    PlayerEditorOptions.instance.Saved();
                }

                GUILayout.FlexibleSpace();
                search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(300));
                GUILayout.EndHorizontal();
            }
        }


        void DrawingSceneEditor()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical("OL box NoExpand", GUILayout.Width(300), GUILayout.Height(position.height - 30));
                    {
                        listScroll = GUILayout.BeginScrollView(listScroll);
                        {
                            DrawingSceneList();
                            GUILayout.EndScrollView();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }


                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical("OL box NoExpand", GUILayout.Width(position.width - 310), GUILayout.Height(position.height - 30));
                    {
                        manifestScroll = GUILayout.BeginScrollView(manifestScroll, false, true);
                        {
                            DrawingSceneOptions();
                            GUILayout.EndScrollView();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawingSceneList()
        {
            if (PlayerEditorOptions.instance.players is null || PlayerEditorOptions.instance.players.Count is 0)
            {
                return;
            }


            for (int i = 0; i < PlayerEditorOptions.instance.players.Count; i++)
            {
                PlayerOptions sceneOptions = PlayerEditorOptions.instance.players[i];
                if (search.IsNullOrEmpty() is false && sceneOptions.name.Equals(search) is false)
                {
                    continue;
                }

                Rect contains = EditorGUILayout.BeginVertical();
                {
                    this.BeginColor(sceneOptions == selection ? Color.cyan : GUI.color);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label($"{sceneOptions.name}", "LargeBoldLabel");
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                        this.EndColor();
                    }

                    GUILayout.Space(5);
                    this.BeginColor(sceneOptions == selection ? inColor : outColor);
                    {
                        GUILayout.Box("", "WhiteBackground", GUILayout.Width(300), GUILayout.Height(1));
                        this.EndColor();
                    }
                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        selection = sceneOptions;
                        this.Repaint();
                    }

                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Build"), false, () => { });
                        menu.AddItem(new GUIContent("Delete"), false, () =>
                        {
                            PlayerEditorOptions.instance.players.Remove(sceneOptions);
                            PlayerEditorOptions.instance.Saved();
                            this.Repaint();
                        });
                        menu.ShowAsContext();
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }
        }

        void DrawingSceneOptions()
        {
            if (selection is null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            selection.name = EditorGUILayout.TextField("Scene Name", selection.name);
            if (EditorGUI.EndChangeCheck())
            {
                PlayerEditorOptions.instance.Saved();
            }
        }
    }
}
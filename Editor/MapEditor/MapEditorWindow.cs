using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ZEngine.Editor.MapEditor
{
    public class MapEditorWindow : EditorWindow
    {
        [MenuItem("Game/Map Create")]
        public static void Open()
        {
            GetWindow<MapEditorWindow>(false, "Map Create", true);
        }

        private string search;
        private Vector2 listScroll;
        private Vector2 manifestScroll;
        private SceneOptions selection;
        private Color inColor = new Color(1f, 0.92f, 0.01f, .8f);
        private Color outColor = new Color(0, 0, 0, 0.2f);

        private void OnEnable()
        {
            if (MapOptions.instance.sceneList is null)
            {
                MapOptions.instance.sceneList = new List<SceneOptions>();
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
                    MapOptions.instance.sceneList.Add(new SceneOptions());
                    MapOptions.instance.Saved();
                }

                GUILayout.FlexibleSpace();
                search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(300));
                if (GUILayout.Button("Add Layer", EditorStyles.toolbarButton))
                {
                    selection.layers.Add(new SceneLayer());
                }

                if (GUILayout.Button("Create", EditorStyles.toolbarButton))
                {
                    CreateMap();
                }

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
            if (MapOptions.instance.sceneList is null || MapOptions.instance.sceneList.Count is 0)
            {
                return;
            }


            for (int i = 0; i < MapOptions.instance.sceneList.Count; i++)
            {
                SceneOptions sceneOptions = MapOptions.instance.sceneList[i];
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
                            MapOptions.instance.sceneList.Remove(sceneOptions);
                            MapOptions.instance.Saved();
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
            selection.size = EditorGUILayout.Vector2Field("Size", selection.size);

            if (selection.layers is null)
            {
                selection.layers = new List<SceneLayer>();
                MapOptions.instance.Saved();
            }


            if (selection.layers.Count is 0)
            {
                GUILayout.Label("No Layer Data");
            }
            else
            {
                for (int i = selection.layers.Count - 1; i >= 0; i--)
                {
                    SceneLayer layer = selection.layers[i];
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    layer.sprite = (Sprite)EditorGUILayout.ObjectField(layer.sprite, typeof(Sprite), false, GUILayout.Width(100), GUILayout.Height(100));
                    GUILayout.BeginVertical();
                    layer.name = EditorGUILayout.TextField("Name", layer.name);
                    layer.layer = EditorGUILayout.IntField("Layer", layer.layer);
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                MapOptions.instance.Saved();
            }
        }

        void CreateMap()
        {
            if (selection is null)
            {
                return;
            }

            int count = (int)selection.size.x * (int)selection.size.y;
            for (int i = 0; i < count; i++)
            {
                SpriteRenderer renderer = new GameObject().AddComponent<SpriteRenderer>();
                float m = Mathf.PerlinNoise(i % selection.size.x, i / selection.size.y);
                SceneLayer layer = selection.layers[Random.Range(0, selection.layers.Count)];
                renderer.sprite = layer.sprite;
                float offset = (i / (int)selection.size.y) % 2 == 0 ? +0.309f : +0f;
                renderer.transform.position = new Vector3(i % (int)selection.size.x * 0.642f + offset, i / (int)selection.size.y * 0.429f, 0);
                renderer.sortingLayerID = layer.layer;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
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
                    manifestScroll = new Vector2(0, 1);
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
            selection.offset = EditorGUILayout.Vector2Field("Offset", selection.offset);
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
                    layer.weight = EditorGUILayout.Slider("Weight", layer.weight, 0, 1);
                    GUILayout.EndVertical();
                    if (GUILayout.Button("Del"))
                    {
                        selection.layers.Remove(layer);
                        MapOptions.instance.Saved();
                        this.Repaint();
                    }

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
            GameObject scene = new GameObject(selection.name);
            for (int i = 0; i < count; i++)
            {
                SpriteRenderer renderer = new GameObject().AddComponent<SpriteRenderer>();
                float m = (float)PerlinNoise(i % selection.size.x, i / selection.size.y); //Math.Clamp(Mathf.PerlinNoise(i % selection.size.x, i / selection.size.y), 0, 1f);
                SceneLayer layer = selection.layers.OrderBy(x => Math.Abs(x.weight - m)).First();
                renderer.sprite = layer.sprite;
                float offset = (i / (int)selection.size.y) % 2 == 0 ? selection.offset.x / 2f : 0f;
                renderer.transform.SetParent(scene.transform);
                renderer.transform.position = new Vector3(i % (int)selection.size.x * selection.offset.x + offset, i / (int)selection.size.y * selection.offset.y, 0);
            }
        }


        float persistence = 0.50f;
        int Number_Of_Octaves = 4;


        double Noise(int x, int y) // 根据(x,y)获取一个初步噪声值
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;
            return (1.0 - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0);
        }

        double SmoothedNoise(int x, int y) //光滑噪声
        {
            double corners = (Noise(x - 1, y - 1) + Noise(x + 1, y - 1) + Noise(x - 1, y + 1) + Noise(x + 1, y + 1)) / 16;
            double sides = (Noise(x - 1, y) + Noise(x + 1, y) + Noise(x, y - 1) + Noise(x, y + 1)) / 8;
            double center = Noise(x, y) / 4;
            return corners + sides + center;
        }

        double Cosine_Interpolate(double a, double b, double x) // 余弦插值
        {
            double ft = x * 3.1415927;
            double f = (1 - Mathf.Cos((float)ft)) * 0.5;
            return a * (1 - f) + b * f;
        }

        double Linear_Interpolate(double a, double b, double x) //线性插值
        {
            return a * (1 - x) + b * x;
        }

        double InterpolatedNoise(float x, float y) // 获取插值噪声
        {
            int integer_X = (int)x;
            float fractional_X = x - integer_X;
            int integer_Y = (int)y;
            float fractional_Y = y - integer_Y;
            double v1 = SmoothedNoise(integer_X, integer_Y);
            double v2 = SmoothedNoise(integer_X + 1, integer_Y);
            double v3 = SmoothedNoise(integer_X, integer_Y + 1);
            double v4 = SmoothedNoise(integer_X + 1, integer_Y + 1);
            double i1 = Cosine_Interpolate(v1, v2, fractional_X);
            double i2 = Cosine_Interpolate(v3, v4, fractional_X);
            return Cosine_Interpolate(i1, i2, fractional_Y);
        }

        double PerlinNoise(float x, float y) // 最终调用：根据(x,y)获得其对应的PerlinNoise值
        {
            double noise = 0;
            double p = persistence;
            int n = Number_Of_Octaves;
            for (int i = 0; i < n; i++)
            {
                double frequency = Math.Pow(2, i);
                double amplitude = Math.Pow(p, i);
                noise = noise + InterpolatedNoise(x * (float)frequency, y * (float)frequency) * amplitude;
            }

            return noise;
        }
    }
}
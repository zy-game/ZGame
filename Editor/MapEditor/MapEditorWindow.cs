using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ZEngine.Editor.MapEditor
{
    public class MapEditorWindow : EngineCustomEditor
    {
        [MenuItem("工具/编辑器/地图编辑器")]
        public static void Open()
        {
            GetWindow<MapEditorWindow>(false, "地图编辑器", true);
        }

        protected override void Actived()
        {
            if (MapOptions.instance.sceneList is null || MapOptions.instance.sceneList.Count is 0)
            {
                MapOptions.instance.sceneList = new List<SceneOptions>();
                return;
            }

            foreach (var VARIABLE in MapOptions.instance.sceneList)
            {
                AddDataItem(VARIABLE.name, VARIABLE);
            }
        }

        protected override void CreateNewItem()
        {
            SceneOptions options = new SceneOptions() { name = "未命名" };
            MapOptions.instance.sceneList.Add(options);
            AddDataItem(options.name, options);
            SaveChanged();
        }

        protected override void SaveChanged()
        {
            MapOptions.instance.Saved();
        }

        protected override void DrawingItemDataView(object data, float width)
        {
            SceneOptions options = (SceneOptions)data;
            options.name = EditorGUILayout.TextField(new GUIContent("配置名"), options.name);
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("地图大小"));
            GUILayout.FlexibleSpace();
            options.size = EditorGUILayout.Vector2IntField("", options.size, GUILayout.Width(width / 2));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("地块大小"));
            GUILayout.FlexibleSpace();
            options.tileSize = EditorGUILayout.Vector2Field("", options.tileSize, GUILayout.Width(width / 2));
            GUILayout.EndHorizontal();
            options.type = (MapType)EditorGUILayout.EnumPopup(new GUIContent("地图类型"), options.type);
            options.direction = (MapTileDirection)EditorGUILayout.EnumPopup(new GUIContent("地块方向"), options.direction);
            if (options.layers is null)
            {
                options.layers = new List<MapLayerOptions>();
            }

            GUILayout.BeginHorizontal(GUI_STYLE_BOX_BACKGROUND);
            GUILayout.Label("Layers", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", GUI_STYLE_ADD_BUTTON))
            {
                options.layers.Add(new MapLayerOptions() { name = "Map Layer" + options.layers.Count });
                this.Repaint();
            }

            GUILayout.EndHorizontal();
            for (int i = options.layers.Count - 1; i >= 0; i--)
            {
                var VARIABLE = options.layers[i];
                Rect layerRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.BeginHorizontal();
                    {
                        VARIABLE.flodout = EditorGUILayout.Foldout(VARIABLE.flodout, VARIABLE.name);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("", GUI_STYLE_MINUS))
                        {
                            options.layers.Remove(VARIABLE);
                            this.Repaint();
                        }
                    }
                    GUILayout.EndHorizontal();
                    if (VARIABLE.flodout)
                    {
                        DrawingMapLayerOptions(options, VARIABLE);
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("构建地图"))
            {
                OnBuild(options);
            }

            if (GUILayout.Button("删除配置"))
            {
                MapOptions.instance.sceneList.Remove(options);
                SaveChanged();
                this.Repaint();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawingMapLayerOptions(SceneOptions sceneOptions, MapLayerOptions mapLayerOptions)
        {
            mapLayerOptions.name = EditorGUILayout.TextField(new GUIContent("Name"), mapLayerOptions.name);
            mapLayerOptions.layer = EditorGUILayout.IntField(new GUIContent("Sort Layer"), mapLayerOptions.layer);
            if (mapLayerOptions.tiles is null)
            {
                mapLayerOptions.tiles = new List<MapTileOptions>();
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                for (int i = mapLayerOptions.tiles.Count - 1; i >= 0; i--)
                {
                    var VARIABLE = mapLayerOptions.tiles[i];
                    Rect tileRect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        if (sceneOptions.type == MapType.M3D)
                        {
                            VARIABLE.mapObject = EditorGUILayout.ObjectField(VARIABLE.mapObject, typeof(GameObject), false, GUILayout.Width(100), GUILayout.Height(100));
                        }
                        else
                        {
                            VARIABLE.mapObject = EditorGUILayout.ObjectField(VARIABLE.mapObject, typeof(Sprite), false, GUILayout.Width(100), GUILayout.Height(100));
                        }

                        GUILayout.Space(20);
                        DrawingDriectionOptions(sceneOptions, VARIABLE);
                        GUILayout.FlexibleSpace();
                        VARIABLE.weight = EditorGUILayout.Slider(new GUIContent("权重"), VARIABLE.weight, 0, 1);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("", GUI_STYLE_MINUS))
                        {
                            mapLayerOptions.tiles.Remove(VARIABLE);
                            this.Repaint();
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("", GUI_STYLE_ADD_BUTTON))
                    {
                        mapLayerOptions.tiles.Add(new MapTileOptions());
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawingDriectionOptions(SceneOptions sceneOptions, MapTileOptions options)
        {
            if (options.dirctions is null || options.dirctions.Length != 9)
            {
                options.dirctions = new bool[9];
                options.dirctions[4] = true;
            }

            GUILayout.BeginVertical();
            {
                GUILayout.Space(10);
                for (int i = 0; i < 9; i++)
                {
                    if (i % 3 == 0)
                    {
                        GUILayout.BeginHorizontal();
                    }

                    if (GUILayout.Button("", options.dirctions[i] ? "WinBtnMaxMac" : "WinBtnInactiveMac", GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        options.dirctions[i] = !options.dirctions[i];
                        options.dirctions[i] = i == 4 ? true : options.dirctions[i];
                        this.Repaint();
                    }

                    if (i % 3 == 2)
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private void OnBuild(SceneOptions options)
        {
            GameObject gameObject = GameObject.Find(options.name);
            if (gameObject != null)
            {
                GameObject.DestroyImmediate(gameObject);
            }

            gameObject = new GameObject(options.name);
            Grid grid = gameObject.AddComponent<Grid>();
            grid.cellSize = options.tileSize;
            switch (options.direction)
            {
                case MapTileDirection.D4:
                    grid.cellLayout = GridLayout.CellLayout.Rectangle;
                    break;
                case MapTileDirection.D6:
                    grid.cellLayout = GridLayout.CellLayout.Hexagon;
                    break;
            }

            options.layers.Sort((a, b) => a.layer > b.layer ? 1 : -1);
            for (int i = 0; i < options.layers.Count; i++)
            {
                GameObject layer = new GameObject(options.layers[i].name);
                Tilemap tilemap = layer.AddComponent<Tilemap>();
            }
        }
    }
}
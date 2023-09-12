using System;
using System.Collections.Generic;
using System.IO;
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

        private SceneOptions options;

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

            options = MapOptions.instance.sceneList.FirstOrDefault();
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
            options = (SceneOptions)data;
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

            options.lacunarity = EditorGUILayout.Slider("偏移值", options.lacunarity, 0, 1);
            options.removeSeparateTileNumberOfTimes = EditorGUILayout.IntSlider("移除孤岛次数", options.removeSeparateTileNumberOfTimes, 0, 10);

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
                    GUILayout.BeginHorizontal(GUI_STYLE_BOX_BACKGROUND);
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
                        DrawingMapLayerOptions(VARIABLE);
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("构建地图"))
            {
                OnBuild();
            }

            if (GUILayout.Button("删除配置"))
            {
                MapOptions.instance.sceneList.Remove(options);
                SaveChanged();
                this.Repaint();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawingMapLayerOptions(MapLayerOptions mapLayerOptions)
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
                        if (options.type == MapType.M3D)
                        {
                            VARIABLE.gameObject = (GameObject)EditorGUILayout.ObjectField(VARIABLE.gameObject, typeof(GameObject), false, GUILayout.Width(100), GUILayout.Height(100));
                        }
                        else
                        {
                            if (VARIABLE.sprite == null)
                            {
                                if (Directory.Exists("Assets/Map/Cache/") is false)
                                {
                                    Directory.CreateDirectory("Assets/Map/Cache/");
                                }

                                string path = "Assets/Map/Cache/" + Guid.NewGuid().ToString().Replace("-", "") + ".asset";
                                AssetDatabase.CreateAsset(new Tile(), path);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                                VARIABLE.sprite = AssetDatabase.LoadAssetAtPath<Tile>(path);
                            }

                            VARIABLE.sprite.sprite = (Sprite)EditorGUILayout.ObjectField(VARIABLE.sprite.sprite, typeof(Sprite), false, GUILayout.Width(100), GUILayout.Height(100));
                        }

                        GUILayout.Space(20);
                        DrawingDriectionOptions(VARIABLE);
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

        private void DrawingDriectionOptions(MapTileOptions options)
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

        private void OnBuild()
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
                if (options.layers[i].tiles is null || options.layers[i].tiles.Count is 0)
                {
                    continue;
                }

                GameObject layer = new GameObject(options.layers[i].name);
                layer.SetParent(gameObject, Vector3.zero, Vector3.zero, Vector3.one);
                Tilemap tilemap = layer.AddComponent<Tilemap>();
                layer.AddComponent<TilemapRenderer>();
                float[,] mapData = new float[options.size.x, options.size.y];
                GenerateMapData(mapData);
                for (int j = 0; j < options.removeSeparateTileNumberOfTimes; j++)
                {
                    if (!RemoveSeparateTile(mapData)) // 如果本次操作什么都没有处理，则不进行循环
                    {
                        break;
                    }
                }

                GenerateTileMap(mapData, options.layers[i], tilemap);
            }
        }

        private void GenerateRuleMap()
        {
        }

        private void GenerateMapData(float[,] mapData)
        {
            // 对于种子的应用


            float randomOffset = UnityEngine.Random.Range(-10000, 10000);

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int x = 0; x < options.size.x; x++)
            {
                for (int y = 0; y < options.size.y; y++)
                {
                    float noiseValue = Mathf.PerlinNoise(x * options.lacunarity + randomOffset, y * options.lacunarity + randomOffset);
                    mapData[x, y] = noiseValue;
                    if (noiseValue < minValue) minValue = noiseValue;
                    if (noiseValue > maxValue) maxValue = noiseValue;
                }
            }

            // 平滑到0~1
            for (int x = 0; x < options.size.x; x++)
            {
                for (int y = 0; y < options.size.y; y++)
                {
                    mapData[x, y] = Mathf.InverseLerp(minValue, maxValue, mapData[x, y]);
                }
            }
        }

        private void GenerateTileMap(float[,] mapData, MapLayerOptions layerOptions, Tilemap tilemap)
        {
            // CleanTileMap();
            if (layerOptions.tiles is null || layerOptions.tiles.Count is 0)
            {
                return;
            }

            List<float> weight = layerOptions.tiles.Select(x => x.weight).ToList();

            //地面
            for (int x = 0; x < options.size.x; x++)
            {
                for (int y = 0; y < options.size.y; y++)
                {
                    MapTileOptions mapTile = layerOptions.tiles.OrderBy(s => Mathf.Abs(s.weight - mapData[x, y])).FirstOrDefault();
                    TileBase tile = mapTile.sprite;
                    tilemap.SetTile(new Vector3Int(-(options.size.x / 2) + x, -(options.size.y / 2) + y), tile);
                }
            }
        }

        private int GetTileIndex(float target, float[] layerOptions)
        {
            float[] temp = new float[layerOptions.Length];
            for (int i = 0; i < layerOptions.Length; i++)
            {
                temp[i] = Mathf.Abs(target - layerOptions[i]);
            }

            return 0;
        }


        private bool RemoveSeparateTile(float[,] mapData)
        {
            bool res = false; // 是否是有效的操作
            for (int x = 0; x < options.size.x; x++)
            {
                for (int y = 0; y < options.size.y; y++)
                {
                    // 是地面且只有一个邻居也是地面
                    if (IsGround(mapData, x, y) && GetFourNeighborsGroundCount(mapData, x, y) <= 1)
                    {
                        mapData[x, y] = 0; // 设置为水
                        res = true;
                    }
                }
            }

            return res;
        }

        private int GetFourNeighborsGroundCount(float[,] mapData, int x, int y)
        {
            int count = 0;
            // top
            if (IsInMapRange(x, y + 1) && IsGround(mapData, x, y + 1)) count += 1;
            // bottom
            if (IsInMapRange(x, y - 1) && IsGround(mapData, x, y - 1)) count += 1;
            // left
            if (IsInMapRange(x - 1, y) && IsGround(mapData, x - 1, y)) count += 1;
            // right
            if (IsInMapRange(x + 1, y) && IsGround(mapData, x + 1, y)) count += 1;
            return count;
        }

        private int GetEigthNeighborsGroundCount(float[,] mapData, int x, int y)
        {
            int count = 0;

            // top
            if (IsInMapRange(x, y + 1) && IsGround(mapData, x, y + 1)) count += 1;
            // bottom
            if (IsInMapRange(x, y - 1) && IsGround(mapData, x, y - 1)) count += 1;
            // left
            if (IsInMapRange(x - 1, y) && IsGround(mapData, x - 1, y)) count += 1;
            // right
            if (IsInMapRange(x + 1, y) && IsGround(mapData, x + 1, y)) count += 1;
            if (options.direction == MapTileDirection.D4)
            {
                return count;
            }

            // left top
            if (IsInMapRange(x - 1, y + 1) && IsGround(mapData, x - 1, y + 1)) count += 1;
            // right top
            if (IsInMapRange(x + 1, y + 1) && IsGround(mapData, x + 1, y + 1)) count += 1;
            // left bottom
            if (IsInMapRange(x - 1, y - 1) && IsGround(mapData, x - 1, y - 1)) count += 1;
            // right bottom
            if (IsInMapRange(x + 1, y - 1) && IsGround(mapData, x + 1, y - 1)) count += 1;
            return count;
        }

        public bool IsInMapRange(int x, int y)
        {
            return x >= 0 && x < options.size.x && y >= 0 && y < options.size.y;
        }

        public bool IsGround(float[,] mapData, int x, int y)
        {
            return mapData[x, y] > .5f;
        }

        public void CleanTileMap()
        {
            GameObject gameObject = GameObject.Find(options.name);
            if (gameObject == null)
            {
                return;
            }

            GameObject.DestroyImmediate(gameObject);
        }
    }
}
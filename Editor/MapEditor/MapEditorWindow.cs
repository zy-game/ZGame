using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ZEngine.Editor.MapEditor
{
    public class MapEditorWindow : EngineCustomEditor
    {
        // [MenuItem("工具/编辑器/地图编辑器")]
        public static void Open()
        {
            GetWindow<MapEditorWindow>(false, "地图编辑器", true);
        }

        class Style
        {
            public static readonly GUIContent empty = EditorGUIUtility.TrTextContent("");
            public static readonly GUIContent offsetLabel = EditorGUIUtility.TrTextContent("Offset");
            public static readonly GUIContent rotationLabel = EditorGUIUtility.TrTextContent("Rotation");
            public static readonly GUIContent scaleLabel = EditorGUIUtility.TrTextContent("Scale");
            public static readonly GUIContent configName = EditorGUIUtility.TrTextContent("配置名");
            public static readonly GUIContent mapSize = EditorGUIUtility.TrTextContent("地图大小");
            public static readonly GUIContent tileSize = EditorGUIUtility.TrTextContent("地块大小");
            public static readonly GUIContent layout = EditorGUIUtility.TrTextContent("Layout");
            public static readonly GUIContent swizzle = EditorGUIUtility.TrTextContent("Swizzle");
            public static readonly GUIContent offset = EditorGUIUtility.TrTextContent("噪波偏移值");
            public static readonly GUIContent islandCount = EditorGUIUtility.TrTextContent("遍历孤岛次数");
            public static readonly GUIContent layer = EditorGUIUtility.TrTextContent("Layers");
            public static readonly GUIContent build = EditorGUIUtility.TrTextContent("构建地图");
            public static readonly GUIContent delete = EditorGUIUtility.TrTextContent("构建地图");
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
            options.name = EditorGUILayout.TextField(Style.configName, options.name);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Style.mapSize);
            GUILayout.FlexibleSpace();
            options.size = EditorGUILayout.Vector2IntField(Style.empty, options.size, GUILayout.Width(width / 2));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(Style.tileSize);
            GUILayout.FlexibleSpace();
            options.tileSize = EditorGUILayout.Vector2Field(Style.empty, options.tileSize, GUILayout.Width(width / 2));
            GUILayout.EndHorizontal();
            options.layout = (GridLayout.CellLayout)EditorGUILayout.EnumPopup(Style.layout, options.layout);
            options.swizzle = (GridLayout.CellSwizzle)EditorGUILayout.EnumPopup(Style.swizzle, options.swizzle);
            if (options.layers is null)
            {
                options.layers = new List<MapLayerOptions>();
            }

            options.lacunarity = EditorGUILayout.Slider(Style.offset, options.lacunarity, 0, 1);
            options.removeSeparateTileNumberOfTimes =
                EditorGUILayout.IntSlider(Style.islandCount, options.removeSeparateTileNumberOfTimes, 0, 10);

            GUILayout.BeginHorizontal(GUI_STYLE_BOX_BACKGROUND);
            GUILayout.Label(Style.layer, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Style.empty, GUI_STYLE_ADD_BUTTON))
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
                        if (GUILayout.Button(Style.empty, GUI_STYLE_MINUS))
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
            if (GUILayout.Button(Style.build))
            {
                OnBuild();
            }

            if (GUILayout.Button(Style.delete))
            {
                MapOptions.instance.sceneList.Remove(options);
                SaveChanged();
                this.Repaint();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawingMapLayerOptions(MapLayerOptions layerOptions)
        {
            layerOptions.name = EditorGUILayout.TextField(new GUIContent("Name"), layerOptions.name);
            layerOptions.layer = EditorGUILayout.IntField(new GUIContent("Sort Layer"), layerOptions.layer);
            if (layerOptions.tiles is null)
            {
                layerOptions.tiles = new List<MapTileOptions>();
            }

            layerOptions.animationFrameRate = EditorGUILayout.FloatField("animationFrameRate", layerOptions.animationFrameRate);
            layerOptions.color = EditorGUILayout.ColorField("Color", layerOptions.color);
            layerOptions.tileAnchor = EditorGUILayout.Vector3Field("Tile Anchor", layerOptions.tileAnchor);
            Tilemap.Orientation orientation = (Tilemap.Orientation)EditorGUILayout.EnumPopup("Orientation", layerOptions.orientation);


            EditorGUI.BeginDisabledGroup(layerOptions.orientation != Tilemap.Orientation.Custom);

            Vector3 pos = Round(layerOptions.matrix.GetColumn(3), 3);
            Vector3 euler = Round(layerOptions.matrix.rotation.eulerAngles, 3);
            Vector3 scale = Round(layerOptions.matrix.lossyScale, 3);
            if (layerOptions.orientation != orientation)
            {
                layerOptions.orientation = orientation;
                pos = layerOptions.orientation == Tilemap.Orientation.Custom ? pos : Vector3.zero;
                scale = layerOptions.orientation == Tilemap.Orientation.Custom ? scale : Vector3.one;
                switch (layerOptions.orientation)
                {
                    case Tilemap.Orientation.XY:
                    case Tilemap.Orientation.XZ:
                        euler = new Vector3(layerOptions.orientation == Tilemap.Orientation.XY ? 0 : 90, 0, 0);
                        break;
                    case Tilemap.Orientation.YX:
                    case Tilemap.Orientation.YZ:
                        euler = new Vector3(0, layerOptions.orientation == Tilemap.Orientation.YX ? 180 : 90, 90);
                        break;
                    case Tilemap.Orientation.ZX:
                    case Tilemap.Orientation.ZY:
                        euler = new Vector3(layerOptions.orientation == Tilemap.Orientation.ZX ? 270 : 0, 270, 0);
                        break;
                }
            }

            pos = EditorGUILayout.Vector3Field(Style.offsetLabel, pos);
            euler = EditorGUILayout.Vector3Field(Style.rotationLabel, euler);
            scale = EditorGUILayout.Vector3Field(Style.scaleLabel, scale);
            if (scale.Equals(Vector3.zero))
            {
                scale = Vector3.one;
            }

            layerOptions.matrix = Matrix4x4.TRS(pos, Quaternion.Euler(euler), scale);
            EditorGUI.EndDisabledGroup();

            layerOptions.sortOrder = (TilemapRenderer.SortOrder)EditorGUILayout.EnumPopup("Sort Order", layerOptions.sortOrder);
            layerOptions.mode = (TilemapRenderer.Mode)EditorGUILayout.EnumPopup("Mode", layerOptions.mode);
            layerOptions.detectChunkCullingBounds = (TilemapRenderer.DetectChunkCullingBounds)EditorGUILayout.EnumPopup("Detect Chunk Culling Bounds", layerOptions.detectChunkCullingBounds);
            EditorGUI.BeginDisabledGroup(layerOptions.detectChunkCullingBounds == TilemapRenderer.DetectChunkCullingBounds.Auto);
            layerOptions.chunkCullingBounds = EditorGUILayout.Vector3Field("Chunk Culling Bounds", layerOptions.chunkCullingBounds);

            EditorGUI.EndDisabledGroup();

            layerOptions.maskInteraction = (SpriteMaskInteraction)EditorGUILayout.EnumPopup("Mask Interaction", layerOptions.maskInteraction);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                for (int i = layerOptions.tiles.Count - 1; i >= 0; i--)
                {
                    var VARIABLE = layerOptions.tiles[i];
                    Rect tileRect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        VARIABLE.mapTile = (TileBase)EditorGUILayout.ObjectField(VARIABLE.mapTile, typeof(TileBase), false);
                        GUILayout.Space(20);
                        GUILayout.FlexibleSpace();
                        VARIABLE.weight = EditorGUILayout.Slider(new GUIContent("权重"), VARIABLE.weight, 0, 1);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("", GUI_STYLE_MINUS))
                        {
                            layerOptions.tiles.Remove(VARIABLE);
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
                        layerOptions.tiles.Add(new MapTileOptions());
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        public static int GetMouseChange()
        {
            return Event.current.button == 1 ? -1 : 1;
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
            grid.cellLayout = options.layout;

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
                tilemap.animationFrameRate = options.layers[i].animationFrameRate;
                tilemap.color = options.layers[i].color;
                tilemap.tileAnchor = options.layers[i].tileAnchor;
                tilemap.orientation = options.layers[i].orientation;


                tilemap.orientationMatrix = options.layers[i].matrix;
                TilemapRenderer renderer = layer.AddComponent<TilemapRenderer>();
                renderer.sortingOrder = options.layers[i].layer;
                renderer.sortOrder = options.layers[i].sortOrder;
                renderer.mode = options.layers[i].mode;
                renderer.chunkCullingBounds = options.layers[i].chunkCullingBounds;
                renderer.detectChunkCullingBounds = options.layers[i].detectChunkCullingBounds;
                renderer.maskInteraction = options.layers[i].maskInteraction;
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

        private static Vector3 Round(Vector3 value, int digits)
        {
            float mult = Mathf.Pow(10.0f, (float)digits);
            return new Vector3(
                Mathf.Round(value.x * mult) / mult,
                Mathf.Round(value.y * mult) / mult,
                Mathf.Round(value.z * mult) / mult
            );
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
                    float noiseValue = Mathf.PerlinNoise(x * options.lacunarity + randomOffset,
                        y * options.lacunarity + randomOffset);
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
                    tilemap.SetTile(new Vector3Int(-(options.size.x / 2) + x, -(options.size.y / 2) + y), mapTile.mapTile);
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
            if (options.layout == GridLayout.CellLayout.Rectangle)
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
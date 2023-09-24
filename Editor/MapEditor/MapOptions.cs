using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace ZEngine.Editor.MapEditor
{
    [Config(Localtion.Project)]
    public class MapOptions : SingleScript<MapOptions>
    {
        public List<SceneOptions> sceneList;
    }

    [Serializable]
    public class SceneOptions
    {
        public string name;
        public Vector2Int size;
        public Vector2 tileSize;
        public float lacunarity;
        public int removeSeparateTileNumberOfTimes;
        public GridLayout.CellLayout layout;
        public GridLayout.CellSwizzle swizzle;
        public List<MapLayerOptions> layers;
    }

    [Serializable]
    public class MapLayerOptions
    {
        public string name;
        public int layer;

        public float animationFrameRate;
        public Color color = Color.white;
        public Vector3 tileAnchor = new Vector3(0.5f, 0.5f, 0);
        public Tilemap.Orientation orientation;
        public Matrix4x4 matrix;

        public TilemapRenderer.SortOrder sortOrder;
        public TilemapRenderer.Mode mode;
        public TilemapRenderer.DetectChunkCullingBounds detectChunkCullingBounds;
        public Vector3 chunkCullingBounds;
        public SpriteMaskInteraction maskInteraction;
        public List<MapTileOptions> tiles;
        [NonSerialized] public bool flodout;
    }

    [Serializable]
    public class MapTileOptions
    {
        public string name;
        public int layer;
        public float weight;
        public TileBase mapTile;
    }
}
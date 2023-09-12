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

    public enum MapType : byte
    {
        M2D,
        M3D
    }

    public enum MapTileDirection : byte
    {
        D4 = 4,
        D6 = 6,
    }

    [Serializable]
    public class SceneOptions
    {
        public string name;
        public Vector2Int size;
        public Vector2 tileSize;
        public float lacunarity;
        public int removeSeparateTileNumberOfTimes;
        public MapType type;
        public MapTileDirection direction;
        public List<MapLayerOptions> layers;
    }

    [Serializable]
    public class MapLayerOptions
    {
        public string name;
        public int layer;
        public List<MapTileOptions> tiles;
        [NonSerialized] public bool flodout;
    }

    [Serializable]
    public class MapTileOptions
    {
        public string name;
        public int layer;
        public float weight;
        public Tile sprite;
        public GameObject gameObject;
        public bool[] dirctions;
    }
}
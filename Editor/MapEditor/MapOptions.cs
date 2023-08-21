using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        public Vector2 size;
        public List<SceneLayer> layers;
    }

    [Serializable]
    public class SceneLayer
    {
        public string name;
        public int layer;
        public Sprite sprite;
    }
}
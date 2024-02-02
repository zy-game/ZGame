using System;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor.PSD2GUI
{
    [ResourceReference("Assets/Settings/PSDConfig.asset")]
    public sealed class PSDConfig : SingletonScriptableObject<PSDConfig>
    {
        public List<PSDImport> imports;

        public override void OnAwake()
        {
            if (imports is null)
            {
                imports = new List<PSDImport>();
            }
        }
    }


    [Serializable]
    public sealed class PSDImport
    {
        public Object psd;
        public List<PSDLayer> layers;

        public void Refresh()
        {
            if (layers is null)
            {
                layers = new List<PSDLayer>();
            }

            layers.Clear();
            PsdFile psdFile = new PsdFile(AssetDatabase.GetAssetPath(psd));
            // psdFile.SortLayer();
            foreach (var VARIABLE in psdFile.Layers)
            {
                GenericLayerData(VARIABLE);
            }
        }

        private PSDLayer GenericLayerData(Layer l)
        {
            PSDLayer layer = new PSDLayer()
            {
                children = new List<PSDLayer>(),
                name = l.Name,
                rect = l.Rect,
                active = true
            };
            layer.texture = ImageDecoder.CreateTexture(l, (int)l.Rect.width, (int)l.Rect.height);
            layers.Add(layer);
            if (l.Children.Count > 0)
            {
                foreach (var VARIABLE in l.Children)
                {
                    layers.Add(GenericLayerData(VARIABLE));
                }
            }

            return layer;
        }
    }

    [Serializable]
    public sealed class PSDLayer
    {
        public string name;
        public Rect rect;
        public List<PSDLayer> children;
        public Sprite sprite;
        public Texture2D texture;
        public bool active;
        [NonSerialized] public bool isOn;
    }
}
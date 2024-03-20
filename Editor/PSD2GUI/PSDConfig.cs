using System;
using System.Collections.Generic;
using System.IO;
using PhotoshopFile;
using TMPro;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ZGame.Editor.PSD2GUI
{
    [ResourceReference("Assets/Settings/PSDConfig.asset")]
    public sealed class PSDConfig : BaseConfig<PSDConfig>
    {
        public List<PSDOptions> options;

        public override void OnAwake()
        {
            if (options is null)
            {
                options = new List<PSDOptions>();
            }
        }
    }

    [Serializable]
    public sealed class PSDOptions
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
            foreach (var VARIABLE in psdFile.Layers)
            {
                GenericLayerData(VARIABLE);
            }
        }

        private PSDLayer GenericLayerData(Layer l)
        {
            if (l.Rect.Equals(Rect.zero))
            {
                return null;
            }

            PSDLayer layer = new PSDLayer()
            {
                children = new List<PSDLayer>(),
                name = l.Name,
                rect = l.Rect,
                active = true
            };

            layer.isSprite = l.Text.IsNullOrEmpty();
            layer.content = l.Text;
            layer.texture = ImageDecoder.CreateTexture(l, (int)l.Rect.width, (int)l.Rect.height);
            layers.Add(layer);
            if (l.Children.Count > 0)
            {
                foreach (var VARIABLE in l.Children)
                {
                    var child = GenericLayerData(VARIABLE);
                    if (child is null)
                    {
                        continue;
                    }

                    layers.Add(child);
                }
            }

            return layer;
        }

        public void Export()
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas is null)
            {
                EditorUtility.DisplayDialog("Error", "场景中没有Canvas", "OK");
                return;
            }

            //todo 先把所有图片写入到到文件夹中
            foreach (var VARIABLE in layers)
            {
                Texture2D texture = VARIABLE.texture;
                string dir = "Assets/PSD2GUI/" + psd.name;
                if (Directory.Exists(dir) is false)
                {
                    Directory.CreateDirectory(dir);
                }

                string savePath = dir + "/" + VARIABLE.name + ".png";
                File.WriteAllBytes(savePath, texture.EncodeToPNG());
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                TextureImporter importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.isReadable = true;
                importer.compressionQuality = 100;
                importer.maxTextureSize = Mathf.Max((int)VARIABLE.rect.width, (int)VARIABLE.rect.height);
                importer.SaveAndReimport();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GameObject root = new GameObject("UGUI_" + psd.name);
            root.AddComponent<RectTransform>();
            root.SetParent(canvas.transform, Vector3.zero, Vector3.zero, Vector3.one);
            foreach (var VARIABLE in layers)
            {
                GenericPSDLayerGameObject(root, VARIABLE);
            }
        }

        private void GenericPSDLayerGameObject(GameObject parent, PSDLayer layer)
        {
            GameObject layerObject = new GameObject(layer.name);
            RectTransform rectTransform = layerObject.AddComponent<RectTransform>();
            layerObject.SetParent(parent.transform, Vector3.zero, Vector3.zero, Vector3.one);
            Vector2 pos = (layer.rect.position + layer.rect.size / 2); //- new Vector2(1920, 1080) / 2; // / 2f;
            pos.y = -pos.y;
            rectTransform.localPosition = pos - new Vector2(1920, -1080) / 2;
            rectTransform.sizeDelta = layer.rect.size;

            if (layer.isSprite)
            {
                Image image = layerObject.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PSD2GUI/" + psd.name + "/" + layer.name + ".png");
            }
            else
            {
                TMP_Text text = layerObject.AddComponent<TMPro.TMP_Text>();
                text.text = layer.content;
            }

            if (layer.children.Count > 0)
            {
                foreach (var VARIABLE in layer.children)
                {
                    GenericPSDLayerGameObject(layerObject, VARIABLE);
                }
            }
        }
    }

    [Serializable]
    public sealed class PSDLayer
    {
        public Rect rect;
        public string name;
        public bool active;
        public bool isSprite;
        public string content;
        public PSDLayer parent;
        public Texture2D texture;
        public List<PSDLayer> children;

        public Vector3 position;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 offsetMax;
        public Vector2 offsetMin;
        public Vector2 pivot;
        public Vector2 size;
        [NonSerialized] public bool isOn;
        [NonSerialized] public Rect rectDraw;

        public bool isJoinDrawingRect(Vector2 pos)
        {
            return rectDraw.Contains(pos);
        }
    }
}
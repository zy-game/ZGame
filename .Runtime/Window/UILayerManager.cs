using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public sealed class UILayerManager : Singleton<UILayerManager>
    {
        private Dictionary<byte, Canvas> layers = new Dictionary<byte, Canvas>();

        private Canvas Create(byte layer)
        {
            Canvas canvas = new GameObject("Canvas").AddComponent<Canvas>();
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            canvas.sortingOrder = (int)layer;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels = 0 >> 0;
            canvas.gameObject.layer = 5;
            layers.Add(layer, canvas);
            GameObject.DontDestroyOnLoad(canvas.gameObject);
            return canvas;
        }

        public void SetLayer(byte layer, GameObject gameObject)
        {
            if (layers.TryGetValue(layer, out Canvas canvas) is false)
            {
                canvas = Create(layer);
            }

            gameObject.transform.SetParent(canvas.transform);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        public void Clear(byte layer = 0)
        {
            if (layer == 0)
            {
                foreach (var VARIABLE in layers.Values)
                {
                    GameObject.DestroyImmediate(VARIABLE.gameObject);
                }

                return;
            }

            if (layers.TryGetValue(layer, out Canvas canvas) is false)
            {
                return;
            }

            for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
            }
        }
    }
}
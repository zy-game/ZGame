using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZGame.Game
{
    public sealed class UILayerManager : Singleton<UILayerManager>
    {
        private GameObject gameObject;
        private Dictionary<int, GameObject> layers = new Dictionary<int, GameObject>();

        public UILayerManager()
        {
            gameObject = new GameObject("EventSystem");
            gameObject.AddComponent<EventSystem>();
            gameObject.AddComponent<StandaloneInputModule>();
            GameObject.DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 尝试将物体设置到指定Layer下
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="layer"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="scale"></param>
        public void TrySetup(GameObject gameObject, int layer, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            if (layers.TryGetValue(layer, out GameObject layerRoot) is false)
            {
                layers.Add(layer, layerRoot = CreateLayerRoot(layer, LayerMask.LayerToName(gameObject.layer) == "UI"));
                GameObject.DontDestroyOnLoad(layerRoot);
            }

            gameObject.transform.SetParent(layerRoot.transform);
            gameObject.transform.position = pos;
            gameObject.transform.rotation = Quaternion.Euler(rot);
            gameObject.transform.localScale = scale;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }


        private GameObject CreateLayerRoot(int layer, bool uiLayer)
        {
            if (uiLayer is false)
            {
                return new GameObject("UILayer_" + layer);
            }

            Canvas canvas = new GameObject("UILayer_" + layer).AddComponent<Canvas>();
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            canvas.sortingOrder = layer;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.gameObject.layer = 5;
            canvas.additionalShaderChannels = 0;
            return canvas.gameObject;
        }

        protected override void OnDestroy()
        {
            foreach (var VARIABLE in layers.Values)
            {
                GameObject.DestroyImmediate(VARIABLE);
            }

            layers.Clear();
            GameObject.DestroyImmediate(gameObject);
        }
    }
}
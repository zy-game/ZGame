using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZGame.UI
{
    class UILayers : IReference
    {
        private Canvas rootCanvas;
        private RectTransform canvasRectTransform;
        private List<Layer> map = new();

        public void Release()
        {
            GameObject.DestroyImmediate(rootCanvas.gameObject);
            rootCanvas = null;
        }

        public static UILayers Create()
        {
            UILayers uiLayers = RefPooled.Alloc<UILayers>();
            uiLayers.rootCanvas = new GameObject("UICamera").AddComponent<Canvas>();
            uiLayers.canvasRectTransform = uiLayers.rootCanvas.gameObject.GetComponent<RectTransform>();
            CanvasScaler scaler = uiLayers.rootCanvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = AppCore.resolution;
            uiLayers.rootCanvas.gameObject.AddComponent<GraphicRaycaster>();
            uiLayers.rootCanvas.sortingOrder = 0;
            uiLayers.rootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiLayers.rootCanvas.worldCamera = AppCore.Camera.uiCamera;
            uiLayers.rootCanvas.gameObject.layer = LayerMask.NameToLayer("UI");
            uiLayers.rootCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            uiLayers.rootCanvas.vertexColorAlwaysGammaSpace = true;
            GameObject.DontDestroyOnLoad(uiLayers.rootCanvas.gameObject);
            var eventSystem = new GameObject("EventSystem");
            GameObject.DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            return uiLayers;
        }

        /// 将世界坐标转换成UI坐标
        public Vector3 WorldToScreenPoint(Vector3 worldPosition, Camera camera)
        {
            Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);

// 如果屏幕坐标在相机视野之外（即在屏幕后面），则不进行转换
            if (screenPosition.z < 0) return screenPosition;

// 转换屏幕坐标到Canvas坐标系
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, AppCore.Camera.uiCamera, out canvasPosition);
            return new Vector3(canvasPosition.x, canvasPosition.y, 0);
        }

        public void SetChild(int layer, RectTransform transform)
        {
            if (rootCanvas == null)
            {
                Create();
            }

            Layer layers = map.Find(x => x.layer == layer);
            if (layers is null)
            {
                map.Add(layers = Layer.Create(layer, rootCanvas.transform));
                map.Sort((x, y) => x.layer.CompareTo(y.layer));
                map.ForEach(x => x.root.SetAsLastSibling());
            }

            layers.SetChild(transform);
        }

        class Layer : IReference
        {
            public RectTransform root;

            public int layer { get; private set; }

            public void SetChild(RectTransform transform)
            {
                transform.gameObject.SetParent(root.transform, Vector3.zero, Vector3.zero, Vector3.one);
                transform.SetRectTransformSizeDelta(Vector2.zero);
                transform.SetRectTransformAnchoredPosition(Vector2.zero);
                transform.SetAsLastSibling();
            }

            public static Layer Create(int layer, Transform parent)
            {
                Layer layers = RefPooled.Alloc<Layer>();
                layers.layer = layer;
                layers.root = new GameObject("Layer " + layer).AddComponent<RectTransform>();
                layers.root.gameObject.SetParent(parent, Vector3.zero, Vector3.zero, Vector3.one);
                layers.root.anchorMin = Vector2.zero;
                layers.root.anchorMax = Vector2.one;
                layers.root.pivot = Vector2.one / 2;
                layers.root.SetRectTransformSizeDelta(Vector2.zero);
                layers.root.SetRectTransformAnchoredPosition(Vector2.zero);
                layers.root.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return layers;
            }

            public void Release()
            {
                layer = 0;
                GameObject.DestroyImmediate(root);
            }
        }
    }
}
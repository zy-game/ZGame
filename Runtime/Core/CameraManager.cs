using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ZGame
{
    public sealed class CameraManager : IManager
    {
        public Camera main { get; }
        private List<CameraItem> cameras = new List<CameraItem>();
        private Dictionary<int, GameObject> layers = new Dictionary<int, GameObject>();
        private UniversalAdditionalCameraData universalAdditionalCameraData;

        class CameraItem
        {
            public int index;
            public Camera camera;
        }

        public CameraManager()
        {
            main = new GameObject("MainCamera").AddComponent<Camera>();
            main.orthographic = false;
            main.backgroundColor = Color.black;
            main.cullingMask = 0;
            universalAdditionalCameraData = main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            universalAdditionalCameraData.renderType = CameraRenderType.Base;
            universalAdditionalCameraData.volumeLayerMask = 0;
            GameObject gameObject = new GameObject("EventSystem");
            gameObject.AddComponent<EventSystem>();
            gameObject.AddComponent<StandaloneInputModule>();
            GameObject.DontDestroyOnLoad(main.gameObject);
            GameObject.DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 创建新相机
        /// </summary>
        /// <param name="name"></param>
        /// <param name="layer"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public Camera NewCamera(string name, int layer, params string[] renderLayers)
        {
            if (renderLayers.Length == 0)
            {
                renderLayers = new[] { "Default" };
            }

            Camera camera = new GameObject(name).AddComponent<Camera>();
            UniversalAdditionalCameraData cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderType = CameraRenderType.Overlay;
            camera.cullingMask = LayerMask.GetMask(renderLayers);
            cameraData.volumeLayerMask = LayerMask.GetMask(renderLayers);
            cameras.Add(new CameraItem() { index = layer, camera = camera });
            GameObject.DontDestroyOnLoad(camera.gameObject);

            Refresh();
            return camera;
        }

        /// <summary>
        /// 刷新相机
        /// </summary>
        public void Refresh()
        {
            universalAdditionalCameraData.cameraStack.Clear();
            cameras.Sort((a, b) => a.index > b.index ? 1 : -1);
            cameras.ForEach(x => universalAdditionalCameraData.cameraStack.Add(x.camera));
        }

        /// <summary>
        /// 获取指定名字的相机
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Camera GetCamera(string name)
        {
            CameraItem item = cameras.Find(x => x.camera.name == name);
            if (item == null)
            {
                return default;
            }

            return item.camera;
        }

        /// <summary>
        /// 获取指定层级的相机
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public Camera GetCamera(int layer)
        {
            CameraItem item = cameras.Find(x => x.index == layer);
            if (item == null)
            {
                return default;
            }

            return item.camera;
        }

        /// <summary>
        /// 移除相机
        /// </summary>
        /// <param name="name"></param>
        public void RemoveCamera(string name)
        {
            CameraItem item = cameras.Find(x => x.camera.name == name);
            if (item == null)
            {
                return;
            }

            GameObject.DestroyImmediate(item.camera.gameObject);
            cameras.Remove(item);
            Refresh();
        }

        /// <summary>
        /// 移除相机
        /// </summary>
        /// <param name="layer"></param>
        public void RemoveCamera(int layer)
        {
            CameraItem item = cameras.Find(x => x.index == layer);
            if (item == null)
            {
                return;
            }

            GameObject.DestroyImmediate(item.camera.gameObject);
            cameras.Remove(item);
            Refresh();
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

            Transform parent = layerRoot.transform;
            gameObject.transform.SetParent(parent);
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
                return new GameObject("layer_root_" + layer);
            }

            Canvas canvas = new GameObject("layer_root_" + layer).AddComponent<Canvas>();
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


        public void Dispose()
        {
            foreach (var VARIABLE in universalAdditionalCameraData.cameraStack)
            {
                GameObject.DestroyImmediate(VARIABLE.gameObject);
            }

            foreach (var VARIABLE in layers.Values)
            {
                GameObject.DestroyImmediate(VARIABLE);
            }

            layers.Clear();
            GameObject.DestroyImmediate(main.gameObject);
            universalAdditionalCameraData.cameraStack.Clear();
        }
    }
}
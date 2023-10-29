using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame
{
    public interface ILayer : IDisposable
    {
        int layer { get; }
        string name { get; }
        Camera camera { get; }
        Canvas canvas { get; }
        void Setup(GameObject gameObject, Vector3 pos, Vector3 rot, Vector3 scale);
        GameObject Find(string name);

        public static ILayer Create(string name, Camera camera)
        {
            return Create(name, camera, -1);
        }

        public static ILayer Create(string name, Camera camera, int layer)
        {
            Canvas canvas = default;
            if (layer >= 0)
            {
                canvas = new GameObject("Canvas").AddComponent<Canvas>();
                CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                canvas.sortingOrder = (int)layer;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.additionalShaderChannels = 0 >> 0;
                canvas.gameObject.layer = 5;
                GameObject.DontDestroyOnLoad(canvas.gameObject);
            }

            return new RenderLayerHandle()
            {
                name = name,
                layer = layer,
                camera = camera,
                canvas = canvas
            };
        }

        class RenderLayerHandle : ILayer
        {
            public int layer { get; set; }
            public string name { get; set; }
            public Camera camera { get; set; }
            public Canvas canvas { get; set; }

            public void Setup(GameObject gameObject, Vector3 pos, Vector3 rot, Vector3 scale)
            {
                Transform parent = canvas == null ? camera.transform : canvas.transform;
                gameObject.transform.SetParent(parent);
                gameObject.transform.position = pos;
                gameObject.transform.rotation = Quaternion.Euler(rot);
                gameObject.transform.localScale = scale;
            }

            public GameObject Find(string name)
            {
                Transform parent = canvas == null ? camera.transform : canvas.transform;
                Transform transform = parent.Find(name);
                if (transform == null)
                    return default;
                return transform.gameObject;
            }

            public void Dispose()
            {
                if (camera != null)
                {
                    GameObject.DestroyImmediate(camera.gameObject);
                }

                if (canvas != null)
                {
                    GameObject.DestroyImmediate(canvas.gameObject);
                }
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ZGame.Game
{
    public class CameraManager : Singleton<CameraManager>
    {
        class CameraItem
        {
            public int index;
            public Camera camera;
        }

        private Camera _main;
        public Camera main => _main;
        private List<CameraItem> cameras = new List<CameraItem>();
        private UniversalAdditionalCameraData universalAdditionalCameraData;

        protected override void OnAwake()
        {
            SceneManager.sceneLoaded += LoadSceneComplete;
        }

        private void LoadSceneComplete(Scene scene, LoadSceneMode mode)
        {
            List<Camera> subCameras = new List<Camera>();
            Camera sceneMainCamera = default;
            foreach (var VARIABLE in scene.GetRootGameObjects())
            {
                UniversalAdditionalCameraData universalAdditionalCameraData = VARIABLE.GetComponentInChildren<UniversalAdditionalCameraData>();
                if (universalAdditionalCameraData == null)
                {
                    continue;
                }

                if (universalAdditionalCameraData.renderType == CameraRenderType.Overlay)
                {
                    subCameras.Add(VARIABLE.GetComponent<Camera>());
                    subCameras.AddRange(universalAdditionalCameraData.cameraStack);
                }
                else
                {
                    if (sceneMainCamera != null)
                    {
                        Debug.LogError("出现多个主摄像机：" + scene.name);
                        continue;
                    }

                    sceneMainCamera = VARIABLE.GetComponent<Camera>();
                }
            }

            if (sceneMainCamera != null)
            {
                SetMainCamera(sceneMainCamera);
            }

            int layer = cameras.Count > 0 ? cameras.Max(x => x.index) : 0;
            foreach (var VARIABLE in subCameras)
            {
                layer++;
                SetSubCamera(VARIABLE, layer);
            }
        }

        public void SetMainCamera(Camera camera = null)
        {
            if (_main != null)
            {
                GameObject.DestroyImmediate(_main.gameObject);
            }

            if (camera == null)
            {
                camera = new GameObject("MainCamera").AddComponent<Camera>();
                camera.orthographic = false;
                camera.backgroundColor = Color.black;
                camera.cullingMask = 0;
                universalAdditionalCameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                universalAdditionalCameraData.renderType = CameraRenderType.Base;
                universalAdditionalCameraData.volumeLayerMask = 0;
            }

            _main = camera;
            Refresh();
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
            camera.gameObject.AddComponent<ZGame.BevaviourScriptable>().onDestroy.AddListener(() => { Remove2(name); });
            camera.cullingMask = LayerMask.GetMask(renderLayers);
            SetSubCamera(camera, layer, renderLayers);
            Refresh();
            return camera;
        }

        public void SetSubCamera(Camera camera, int layer, params string[] renderLayers)
        {
            UniversalAdditionalCameraData cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderType = CameraRenderType.Overlay;
            if (renderLayers.Length > 0)
            {
                cameraData.volumeLayerMask = LayerMask.GetMask(renderLayers);
            }

            cameras.Add(new CameraItem() { index = layer, camera = camera });
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

            cameras.Remove(item);
            GameObject.DestroyImmediate(item.camera.gameObject);
            Refresh();
        }

        private void Remove2(string name)
        {
            CameraItem item = cameras.Find(x => x.camera.name == name);
            if (item == null)
            {
                return;
            }

            cameras.Remove(item);
            Refresh();
        }

        protected override void OnDestroy()
        {
            foreach (var VARIABLE in cameras)
            {
                GameObject.DestroyImmediate(VARIABLE.camera.gameObject);
            }

            cameras.Clear();
            if (_main == null)
            {
                return;
            }

            GameObject.DestroyImmediate(_main.gameObject);
        }
    }
}
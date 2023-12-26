using System;
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
            public int sort;
            public Camera camera;
        }

        private Camera _main;
        private List<(int, Camera)> cameraList = new();
        private UniversalAdditionalCameraData universalAdditionalCameraData;
        public Camera main => _main;

        //public Camera[] cameras=> cameras.Select(x=>x.camera).ToArray();

        protected override void OnAwake()
        {
            SceneManager.sceneLoaded += LoadSceneComplete;
        }

        private void LoadSceneComplete(Scene scene, LoadSceneMode mode)
        {
            List<Camera> subCameras = new List<Camera>();

            List<UniversalAdditionalCameraData> universalAdditionalCameraDatas = new List<UniversalAdditionalCameraData>();
            foreach (var VARIABLE in scene.GetRootGameObjects())
            {
                universalAdditionalCameraDatas.AddRange(VARIABLE.GetComponentsInChildren<UniversalAdditionalCameraData>());
            }

            UniversalAdditionalCameraData mainCamera = universalAdditionalCameraDatas.Find(x => x.renderType == CameraRenderType.Base);
            if (mainCamera != null)
            {
                SetMainCamera(mainCamera.GetComponent<Camera>());
                return;
            }

            foreach (var VARIABLE in universalAdditionalCameraDatas)
            {
                if (universalAdditionalCameraData.renderType == CameraRenderType.Overlay)
                {
                    SetSubCamera(VARIABLE.GetComponent<Camera>());
                }
            }
        }

        public void SetMainCamera(Camera camera = null)
        {
            if (_main != null)
            {
                GameObject.DestroyImmediate(_main.gameObject);
                Clear();
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

        public void Clear()
        {
            foreach (var VARIABLE in cameraList)
            {
                GameObject.DestroyImmediate(VARIABLE.Item2.gameObject);
            }

            cameraList.Clear();
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
            UniversalAdditionalCameraData cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            if (renderLayers.Length > 0)
            {
                cameraData.volumeLayerMask = LayerMask.GetMask(renderLayers);
            }

            cameraData.renderType = CameraRenderType.Overlay;
            SetSubCamera(camera);
            Refresh();
            return camera;
        }

        public void SetSubCamera(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            int sort = cameraList.Count == 0 ? 0 : cameraList.Max(x => x.Item1);
            sort++;
            cameraList.Add(new(sort, camera));
            Refresh();
        }

        /// <summary>
        /// 刷新相机
        /// </summary>
        public void Refresh()
        {
            universalAdditionalCameraData.cameraStack.Clear();
            cameraList.Sort((a, b) => a.Item1 > b.Item1 ? 1 : -1);
            foreach (var VARIABLE in cameraList)
            {
                universalAdditionalCameraData.cameraStack.Add(VARIABLE.Item2);
            }
        }

        /// <summary>
        /// 获取指定名字的相机
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Camera GetCamera(string name)
        {
            foreach (var VARIABLE in cameraList)
            {
                if (VARIABLE.Item2.name == name)
                {
                    return VARIABLE.Item2;
                }
            }

            return default;
        }

        /// <summary>
        /// 移除相机
        /// </summary>
        /// <param name="name"></param>
        public void RemoveCamera(string name)
        {
            Camera item = GetCamera(name);
            if (item == null)
            {
                return;
            }

            GameObject.DestroyImmediate(item.gameObject);
            Refresh();
        }

        private void Remove2(string name)
        {
            for (int i = 0; i < cameraList.Count; i++)
            {
                if (cameraList[i].Item2.name == name)
                {
                    cameraList.RemoveAt(i);
                    break;
                }
            }

            Refresh();
        }

        protected override void OnDestroy()
        {
            for (int i = cameraList.Count - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(cameraList[i].Item2.gameObject);
            }

            cameraList.Clear();
            if (_main == null)
            {
                return;
            }

            GameObject.DestroyImmediate(_main.gameObject);
        }
    }
}
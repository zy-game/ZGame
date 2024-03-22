using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ZGame.Game
{
    public sealed class GameWorld : IDisposable
    {
        private int _hour;
        private int _speed;
        private int _minute;
        private int _second;
        private string _name;
        private Camera _camera;
        private Light _light;
        private Skybox skybox;
        private Gradient sunshineGradient;
        private List<Tuple<int, Camera>> subCameras = new();
        private UniversalAdditionalCameraData universalAdditionalCameraData;


        public string name
        {
            get { return _name; }
        }

        /// <summary>
        /// 主相机
        /// </summary>
        public Camera main
        {
            get { return _camera; }
        }

        /// <summary>
        /// 主灯光
        /// </summary>
        public Light mainLight
        {
            get { return _light; }
        }

        /// <summary>
        /// 当前世界时数
        /// </summary>
        public int Hour
        {
            get { return _hour; }
        }

        /// <summary>
        /// 当前世界分数
        /// </summary>
        public int Minute
        {
            get { return _minute; }
        }

        /// <summary>
        /// 当前世界的秒数
        /// </summary>
        public int Second
        {
            get { return _second; }
        }

        public GameWorld(string name)
        {
            _name = name;
            _camera = new GameObject(_name).AddComponent<Camera>();
            universalAdditionalCameraData = main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            universalAdditionalCameraData.renderShadows = false;
            universalAdditionalCameraData.renderType = CameraRenderType.Base;
            main.cullingMask = 0;
            universalAdditionalCameraData.volumeLayerMask = 0;
            main.allowMSAA = false;
            skybox = main.gameObject.AddComponent<Skybox>();
        }

        private void OnFixedUpdate()
        {
            _second += _speed;
            if (_second >= 60)
            {
                _second = 0;
                _minute++;
            }

            if (_minute >= 60)
            {
                _minute = 0;
                _hour++;
            }

            if (_hour >= 24)
            {
                _hour = 0;
            }

            RefreshSunshine();
        }

        /// <summary>
        /// 设置全局光
        /// </summary>
        /// <param name="color"></param>
        /// <param name="layers"></param>
        public void SetupLight(Color color, params string[] layers)
        {
            if (mainLight != null)
            {
                return;
            }

            _light = new GameObject("Main Light").AddComponent<Light>();
            _light.type = LightType.Directional;
            _light.transform.rotation = Quaternion.Euler(50, -30, 0);
            _light.cullingMask = LayerMask.GetMask(layers);
            _light.intensity = 1;
            _light.transform.parent = main.transform;
            _light.renderMode = LightRenderMode.Auto;
            _light.shadows = LightShadows.None;
            _light.color = color;
        }

        /// <summary>
        /// 设置相机输出
        /// </summary>
        /// <param name="texture"></param>
        public void SetupCameraOutput(RenderTexture texture)
        {
            _camera.targetTexture = texture;
            _camera.fieldOfView = 10;
            _camera.clearFlags = CameraClearFlags.Color;
        }

        /// <summary>
        /// 设置主相机的渲染层级
        /// </summary>
        /// <param name="layers"></param>
        public void SetupCameraRenderLayer(params string[] layers)
        {
            if (universalAdditionalCameraData == null || _camera == null)
            {
                return;
            }

            _camera.cullingMask = LayerMask.GetMask(layers);
            universalAdditionalCameraData.volumeLayerMask = LayerMask.GetMask(layers);
        }

        /// <summary>
        /// 设置阳光颜色
        /// </summary>
        /// <param name="gradient"></param>
        public void SetSunshine(Gradient gradient)
        {
            sunshineGradient = gradient;
        }

        private void RefreshSunshine()
        {
            if (sunshineGradient == null)
            {
                return;
            }

            float hourPercent = _hour % 1;
            float minutePercent = _minute % 1;
            float secondPercent = _second % 1;
            Color color = sunshineGradient.Evaluate(hourPercent);
        }

        /// <summary>
        /// 设置时间
        /// </summary>
        /// <param name="house"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="timeSpeed"></param>
        public void SetWorldTime(int house, int minute, int second, int timeSpeed)
        {
            this._hour = house;
            this._minute = minute;
            this._second = second;
            this._speed = timeSpeed;
            RefreshSunshine();
        }

        /// <summary>
        /// 设置天空盒
        /// </summary>
        /// <param name="material"></param>
        public void SetSkybox(Material material)
        {
            skybox.material = material;
            main.clearFlags = CameraClearFlags.Skybox;
        }

        /// <summary>
        /// 关闭天空盒
        /// </summary>
        public void CloseSkybox()
        {
            skybox.enabled = false;
            main.clearFlags = CameraClearFlags.Color;
            main.backgroundColor = Color.clear;
        }

        /// <summary>
        /// 设置子摄像机
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="sort"></param>
        public void SetSubCamera(Camera camera, int sort)
        {
            UniversalAdditionalCameraData universalAdditionalCameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            universalAdditionalCameraData.renderShadows = false;
            universalAdditionalCameraData.renderType = CameraRenderType.Overlay;
            universalAdditionalCameraData.volumeLayerMask = camera.cullingMask;
            subCameras.Add(new Tuple<int, Camera>(sort, camera));
            RefreshSubCamera();
        }

        /// <summary>
        /// 设置子摄像机
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sort"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public Camera SetSubCamera(string name, int sort, params string[] layers)
        {
            Camera camera = new GameObject(name).AddComponent<Camera>();
            camera.transform.parent = main.transform;
            camera.cullingMask = LayerMask.GetMask(layers);
            camera.allowMSAA = false;
            SetSubCamera(camera, sort);
            return camera;
        }

        private void RefreshSubCamera()
        {
            subCameras.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            universalAdditionalCameraData.cameraStack.Clear();
            foreach (var item in subCameras)
            {
                universalAdditionalCameraData.cameraStack.Add(item.Item2);
            }
        }


        public void Dispose()
        {
            foreach (var item in subCameras)
            {
                GameObject.DestroyImmediate(item.Item2.gameObject);
            }

            if (_light != null)
            {
                GameObject.DestroyImmediate(_light.gameObject);
            }

            if (_camera != null)
            {
                GameObject.DestroyImmediate(_camera.gameObject);
            }

            subCameras.Clear();
            sunshineGradient = null;
            skybox = null;
            _light = null;
            _camera = null;
            GC.SuppressFinalize(this);
        }
    }
}
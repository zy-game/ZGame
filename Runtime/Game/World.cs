using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ZGame.Game
{
    public class World : IDisposable
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

        public Camera main
        {
            get { return _camera; }
        }

        public Light mainLight
        {
            get { return _light; }
        }

        public int Hour
        {
            get { return _hour; }
        }

        public int Minute
        {
            get { return _minute; }
        }

        public int Second
        {
            get { return _second; }
        }

        internal World(string name)
        {
            _name = name;
            _camera = new GameObject(name).AddComponent<Camera>();
            universalAdditionalCameraData = main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            universalAdditionalCameraData.renderShadows = false;
            universalAdditionalCameraData.renderType = CameraRenderType.Base;
            main.cullingMask = 0;
            universalAdditionalCameraData.volumeLayerMask = 0;
            main.allowMSAA = false;
            skybox = main.gameObject.AddComponent<Skybox>();
        }

        public void OnFixedUpdate()
        {
            RefreshTime();
        }

        public void SetupGlobalLight(Color color, params string[] layers)
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

        public void SetupMainCameraOutput(RenderTexture texture)
        {
            _camera.targetTexture = texture;
            _camera.fieldOfView = 10;
            _camera.clearFlags = CameraClearFlags.Color;
        }
        
        
        

        public void SetupMainCameraRenderLayer(params string[] layers)
        {
            if (universalAdditionalCameraData == null || _camera == null)
            {
                return;
            }

            _camera.cullingMask = LayerMask.GetMask(layers);
            universalAdditionalCameraData.volumeLayerMask = LayerMask.GetMask(layers);
        }

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

        public void SetWorldTime(int house, int minute, int second, int timeSpeed)
        {
            this._hour = house;
            this._minute = minute;
            this._second = second;
            this._speed = timeSpeed;
            RefreshSunshine();
        }

        public void SetSkybox(Material material)
        {
            skybox.material = material;
            main.clearFlags = CameraClearFlags.Skybox;
        }

        public void CloseSkybox()
        {
            skybox.enabled = false;
            main.clearFlags = CameraClearFlags.Color;
            main.backgroundColor = Color.clear;
        }

        public void SetSubCamera(Camera camera, int sort)
        {
            UniversalAdditionalCameraData universalAdditionalCameraData = main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            universalAdditionalCameraData.renderShadows = false;
            universalAdditionalCameraData.renderType = CameraRenderType.Overlay;
            universalAdditionalCameraData.volumeLayerMask = camera.cullingMask;
            subCameras.Add(new Tuple<int, Camera>(sort, camera));
            RefreshSubCamera();
        }

        public Camera SetSubCamera(string name, int sort, params string[] layers)
        {
            Camera camera = new GameObject(name).AddComponent<Camera>();
            camera.transform.parent = main.transform;
            camera.cullingMask = LayerMask.GetMask(layers);
            camera.allowMSAA = false;
            camera.cullingMask = 0;
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

        public void RefreshTime()
        {
            _second += _speed;
            if (_second < 60)
            {
                return;
            }

            _second = 0;
            _minute++;
            if (_minute < 60)
            {
                return;
            }

            _minute = 0;
            _hour++;
            RefreshSunshine();
            if (_hour >= 24)
            {
                _hour = 0;
            }
        }

        public void Dispose()
        {
            GameObject.DestroyImmediate(_light);
            GameObject.DestroyImmediate(_camera);
            foreach (var item in subCameras)
            {
                GameObject.DestroyImmediate(item.Item2);
            }

            subCameras.Clear();
            sunshineGradient = null;
            skybox = null;
            _light = null;
            _camera = null;
        }
    }
}
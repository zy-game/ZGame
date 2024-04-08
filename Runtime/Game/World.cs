using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using TrueSync;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using ZGame.Networking;
using ZGame.UI;

namespace ZGame.Game
{
    public sealed class World : IReferenceObject
    {
        private string _name;
        private Light _light;
        private Skybox skybox;
        private Camera _camera;
        private int timeSpeed = 1;
        private GameObject mapRoot;
        private DateTime worldTime;
        private CinemachineBrain brain;
        private Gradient sunshineGradient;
        private List<Tuple<int, Camera>> subCameras = new();
        private UniversalAdditionalCameraData mainCameraData;
        private Simulator _simulator;

        // private TrueSyncStats _trueSyncStats;

        /// <summary>
        /// 世界名
        /// </summary>
        public string name => _name;

        /// <summary>
        /// 主相机
        /// </summary>
        public Camera mainCamera => _camera;

        /// <summary>
        /// 主灯光
        /// </summary>
        public Light mainLight => _light;

        /// <summary>
        /// 当前世界的时间
        /// </summary>
        public DateTime time => worldTime;

        /// <summary>
        /// 帧同步模拟器
        /// </summary>
        public Simulator simulator => _simulator;

        /// <summary>
        /// 世界时间流速，每帧流逝的秒数
        /// </summary>
        public int TimeFlowRate
        {
            get => timeSpeed;
            set => timeSpeed = value;
        }

        public static World Create(string name)
        {
            World world = GameFrameworkFactory.Spawner<World>();
            world._name = name;
            world.worldTime = new DateTime();
            world._camera = new GameObject(world._name).AddComponent<Camera>();
            world.brain = world._camera.gameObject.AddComponent<CinemachineBrain>();
            world.brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
            world.mainCameraData = world.mainCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            world.mainCameraData.renderShadows = false;
            world.mainCameraData.renderType = CameraRenderType.Base;
            world.mainCameraData.volumeLayerMask = 0;
            world.mainCamera.cullingMask = 0;
            world.mainCamera.allowMSAA = false;

            world._light = new GameObject("Main Light").AddComponent<Light>();
            world._light.type = LightType.Directional;
            world._light.transform.rotation = Quaternion.Euler(50, -30, 0);
            world._light.intensity = 1;
            world._light.transform.parent = world.mainCamera.transform;
            world._light.renderMode = LightRenderMode.Auto;
            world._light.shadows = LightShadows.None;
            world._light.color = Color.white;
            return world;
        }


        public void OnFixedUpdate()
        {
            _simulator?.FixedUpdate();
            this.worldTime.AddSeconds(timeSpeed);
            RefreshSunshine();
        }

        public void OnUpdate()
        {
            _simulator?.Update();
        }

        public void OnLateUpdate()
        {
        }

        public void OnDarwGizom()
        {
        }

        public void OnGUI()
        {
        }

        public async UniTask OnStartSimulator(string ip, ushort port)
        {
            SimulatorNetworkHandle handle = GameFrameworkFactory.Spawner<SimulatorNetworkHandle>();
            await GameFrameworkEntry.Network.Connect<UdpClient>("127.0.0.1", 8099, handle);
            PhysicsManager.instance = new Physics3DSimulator();
            PhysicsManager.instance.Gravity = new TSVector(0, -10, 0);
            PhysicsManager.instance.SpeculativeContacts = true;
            PhysicsManager.instance.LockedTimeStep = 0.0167;
            PhysicsManager.instance.Init();
            _simulator = Simulator.Create3D(handle);
        }

        /// <summary>
        /// 加载世界场景
        /// </summary>
        /// <param name="mapName"></param>
        public async UniTask SetWorldMapAsync(string mapName)
        {
            mapRoot = await GameFrameworkEntry.VFS.GetGameObjectAsync(mapName);
        }

        /// <summary>
        /// 加载世界场景
        /// </summary>
        /// <param name="sceneName"></param>
        public async UniTask SetWorldSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            await GameFrameworkEntry.VFS.GetSceneAsync(sceneName, UILoading.Show(), mode);
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

            mainLight.color = sunshineGradient.Evaluate(worldTime.Hour / 24f);
        }

        /// <summary>
        /// 设置天空盒
        /// </summary>
        /// <param name="material"></param>
        public void SetSkybox(Material material)
        {
            if (skybox == null)
            {
                skybox = mainCamera.gameObject.AddComponent<Skybox>();
            }

            skybox.material = material;
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }

        /// <summary>
        /// 关闭天空盒
        /// </summary>
        public void CloseSkybox()
        {
            if (skybox == null)
            {
                return;
            }

            skybox.enabled = false;
            mainCamera.clearFlags = CameraClearFlags.Color;
            mainCamera.backgroundColor = Color.clear;
        }

        /// <summary>
        /// 设置子摄像机
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sort"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public Camera SetSubCamera(string name, int sort)
        {
            Camera camera = new GameObject(name).AddComponent<Camera>();
            camera.allowMSAA = false;
            if (camera.TryGetComponent<UniversalAdditionalCameraData>(out UniversalAdditionalCameraData universal) is false)
            {
                universal = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            universal.renderShadows = false;
            universal.renderType = CameraRenderType.Overlay;
            subCameras.Add(new Tuple<int, Camera>(sort, camera));
            subCameras.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            foreach (var item in subCameras)
            {
                mainCameraData.cameraStack.Add(item.Item2);
            }

            return camera;
        }

        /// <summary>
        /// 设置子摄像机渲染层
        /// </summary>
        /// <param name="name"></param>
        /// <param name="layers"></param>
        public void SetSubCameraRenderLayers(string name, params string[] layers)
        {
            Camera main = GetSubCamera(name);
            if (main is null)
            {
                return;
            }

            main.cullingMask = LayerMask.GetMask(layers);
            if (main.gameObject.TryGetComponent<UniversalAdditionalCameraData>(out UniversalAdditionalCameraData universal) is false)
            {
                universal = main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            universal.volumeLayerMask = main.cullingMask;
        }

        /// <summary>
        /// 设置子摄像机位置和旋转
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SetSubCameraPositionAndRotation(string name, Vector3 position, Quaternion rotation)
        {
            Camera main = GetSubCamera(name);
            if (main is null)
            {
                return;
            }

            main.transform.position = position;
            main.transform.rotation = rotation;
        }

        public Camera GetSubCamera(string name)
        {
            Tuple<int, Camera> item = subCameras.FirstOrDefault(x => x.Item2.name == name);
            if (item is null || item.Item2 is null)
            {
                return default;
            }

            return item.Item2;
        }

        public void Release()
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

            GameFrameworkFactory.Release(simulator);
            _simulator = null;
            subCameras.Clear();
            sunshineGradient = null;
            skybox = null;
            _light = null;
            _camera = null;
        }
    }
}
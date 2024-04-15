using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using FixMath.NET;
using TrueSync;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using ZGame.Networking;
using ZGame.UI;

namespace ZGame.Game
{
    class CameraHandle : IReferenceObject
    {
        public void Release()
        {
            
        }
    }

    class LightHandle : IReferenceObject
    {
        public void Release()
        {
            
        }
    }

    class SkyboxHandle : IReferenceObject
    {
        public void Release()
        {
            
        }
    }

    public sealed class World : GameFrameworkModule
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
        private ArchetypeManager _archetypeManager;
        private EntityManager entityManager;
        private SystemManager systemManager;

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


        public override void OnAwake(params object[] args)
        {
            
            _archetypeManager = GameFrameworkFactory.Spawner<ArchetypeManager>();
            entityManager = GameFrameworkFactory.Spawner<EntityManager>();
            systemManager = GameFrameworkFactory.Spawner<SystemManager>();
        }

        public override void Release()
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

            GameFrameworkFactory.Release(_archetypeManager);
            GameFrameworkFactory.Release(systemManager);
            GameFrameworkFactory.Release(entityManager);
            GameFrameworkFactory.Release(simulator);
            _simulator = null;
            subCameras.Clear();
            sunshineGradient = null;
            skybox = null;
            _light = null;
            _camera = null;
        }
        public override void Update()
        {
            systemManager.Update();
        }

        public override void FixedUpdate()
        {
            systemManager.FixedUpdate();
        }

        public override void LateUpdate()
        {
            systemManager.LateUpdate();
        }

        public override void OnDarwGizom()
        {
            systemManager.OnDarwingGizmons();
        }

        public override void OnGUI()
        {
            systemManager.OnGUI();
            entityManager.OnGUI();
            _archetypeManager.OnGUI();
        }

        public void OnFixedUpdate()
        {
            _simulator?.OnFixedUpdate();
            this.worldTime.AddSeconds(timeSpeed);
            RefreshSunshine();
        }

        public void OnUpdate()
        {
            _simulator?.OnUpdate();
        }

        public void OnLateUpdate()
        {
        }

        public async UniTask OnCreateSimulator(string ip, ushort port, Fix64 LockedTimeStep)
        {
            _simulator = await Simulator.Create3DSimulator(LockedTimeStep, ip, port);
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
        
        /// <summary>
        /// 获取所有拥有指定类型组件的实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Entity[] GetEntities<T>() where T : IComponent
        {
            Type type = typeof(T);
            uint[] entitys = _archetypeManager.GetHaveComponentEntityID(type);
            Entity[] entities = new Entity[entitys.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = entityManager.FindEntity(entitys[i]);
            }

            return entities;
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Entity GetEntity(uint id)
        {
            return entityManager.FindEntity(id);
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <returns></returns>
        public Entity CreateEntity()
        {
            return entityManager.CreateEntity();
        }

        /// <summary>
        /// 销毁实体
        /// </summary>
        /// <param name="id"></param>
        public void DestroyEntity(uint id)
        {
            entityManager.DestroyEntity(id);
            _archetypeManager.RemoveEntityComponents(id);
        }

        /// <summary>
        /// 销毁实体
        /// </summary>
        /// <param name="entity"></param>
        public void DestroyEntity(Entity entity)
        {
            DestroyEntity(entity.id);
        }

        /// <summary>
        /// 清理所有实体对象
        /// </summary>
        public void ClearEntity()
        {
            entityManager.Clear();
            _archetypeManager.Clear();
        }

        /// <summary>
        /// 注册逻辑系统
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RegisterGameLogicSystem(Type systemType, params object[] args)
        {
            systemManager.RegisterGameLogicSystem(systemType, args);
        }

        /// <summary>
        /// 注册逻辑系统
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterGameLogicSystem<T>(params object[] args) where T : ISystem
        {
            RegisterGameLogicSystem(typeof(T), args);
        }

        /// <summary>
        /// 卸载逻辑系统
        /// </summary>
        /// <param name="systemType"></param>
        public void UnregisterGameLogicSystem(Type systemType)
        {
            systemManager.UnregisterGameLogicSystem(systemType);
        }

        /// <summary>
        /// 卸载逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnregisterGameLogicSystem<T>() where T : ISystem
        {
            UnregisterGameLogicSystem(typeof(T));
        }

        /// <summary>
        /// 获取指定类型的系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSystem<T>() where T : ISystem
        {
            return (T)GetSystem(typeof(T));
        }

        /// <summary>
        /// 获取指定类型的系统
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ISystem GetSystem(Type type)
        {
            return systemManager.GetSystem(type);
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent AddComponent(Entity entity, Type type)
        {
            return _archetypeManager.AddComponent(entity.id, type);
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent GetComponent(Entity entity, Type type)
        {
            return _archetypeManager.GetComponent(entity.id, type);
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveComponent(Entity entity, Type type)
        {
            _archetypeManager.RemoveComponent(entity.id, type);
        }

        /// <summary>
        /// 获取所有指定类型的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> AllOf<T>() where T : IComponent
        {
            return new ComponentEnumerable<T>(_archetypeManager.GetComponents(typeof(T)));
        }

        /// <summary>
        /// 获取实体上指定的组件
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Of<T>(Entity entity) where T : IComponent
        {
            return (T)_archetypeManager.GetComponent(entity.id, typeof(T));
        }

        /// <summary>
        /// 获取全局组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Single<T>() where T : ISingletonComponent, new()
        {
            return (T)_archetypeManager.GetComponent(0, typeof(T));
        }
    }
}
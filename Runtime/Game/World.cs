using System;
using System.Collections.Generic;
using System.Dynamic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;
using FixMath.NET;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.Networking;
using ZGame.UI;

namespace ZGame.Game
{
    public sealed class World : IReference
    {
        private string _name;
        private int timeSpeed = 1;
        private DateTime worldTime;
        private ISimulator _simulator;
        private EntityManager _entitys;
        private SystemManager _systems;
        private LightManager _lightManager;
        private CameraManager _cameraManager;
        private ArchetypeManager _archetypes;
        private SkyboxManager _skyboxManager;


        /// <summary>
        /// 世界名
        /// </summary>
        public string name => _name;

        /// <summary>
        /// 主相机
        /// </summary>
        public Camera mainCamera => _cameraManager.main;

        /// <summary>
        /// 主灯光
        /// </summary>
        public Light mainLight => _lightManager.main;

        /// <summary>
        /// 当前世界的时间
        /// </summary>
        public DateTime time => worldTime;

        /// <summary>
        /// 帧间隔时间
        /// </summary>
        public Fix64 DeltaTime
        {
            get
            {
                if (_simulator == null)
                {
                    return 0.02f;
                }

                return _simulator.DeltaTime;
            }
        }


        internal static World Create(string name)
        {
            World world = RefPooled.Spawner<World>();
            world._name = name;
            world.worldTime = new DateTime();
            world._archetypes = RefPooled.Spawner<ArchetypeManager>();
            world._entitys = RefPooled.Spawner<EntityManager>();
            world._systems = RefPooled.Spawner<SystemManager>();
            world._lightManager = LightManager.Create(name, Color.white);
            world._cameraManager = CameraManager.Create(name);
            world._skyboxManager = SkyboxManager.Create(name, world._cameraManager.main);
            return world;
        }

        public void Release()
        {
            RefPooled.Release(_skyboxManager);
            RefPooled.Release(_cameraManager);
            RefPooled.Release(_lightManager);
            RefPooled.Release(_archetypes);
            RefPooled.Release(_systems);
            RefPooled.Release(_entitys);
        }

        public void Update()
        {
            _systems.Update();
            _simulator?.Update();
        }

        public void FixedUpdate()
        {
            this.worldTime.AddSeconds(timeSpeed);
            this._lightManager.Refresh(this.time.Hour);
            _systems.FixedUpdate();
            _simulator?.FixedUpdate();
        }


        public void LateUpdate()
        {
            _systems.LateUpdate();
            _cameraManager.LateUpdate();
            _simulator?.LateUpdate();
        }

        public void OnDarwGizom()
        {
            _systems.OnDarwGizmons();
            _simulator.OnDrawGizmos();
        }

        public void OnGUI()
        {
            _systems.OnGUI();
            _entitys.OnGUI();
            _archetypes.OnGUI();
            _simulator?.OnGUI();
        }

        public async UniTask OnCreateSimulator(int cid, string ip, ushort port, int milliseconds)
        {
            this._simulator = await FrameSyncSimulator.Create(cid, ip, port, milliseconds);
        }

        /// <summary>
        /// 设置阳光颜色
        /// </summary>
        /// <param name="gradient"></param>
        public void SetSunshine(Gradient gradient)
        {
            _lightManager.SetSunshine(gradient);
        }

        /// <summary>
        /// 设置天空盒
        /// </summary>
        /// <param name="material"></param>
        public void SetSkybox(Material material)
        {
            _skyboxManager.SetSkybox(material);
        }

        /// <summary>
        /// 关闭天空盒
        /// </summary>
        public void CloseSkybox()
        {
            _skyboxManager.Release();
        }

        /// <summary>
        /// 设置子摄像机
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="sort"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public Camera SetSubCamera(string cameraName, int sort, params string[] layers)
        {
            return _cameraManager.SetSubCamera(cameraName, sort, layers);
        }

        /// <summary>
        /// 设置子摄像机位置和旋转
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SetSubCameraPositionAndRotation(string cameraName, Vector3 position, Quaternion rotation)
        {
            _cameraManager.SetSubCameraPositionAndRotation(cameraName, position, rotation);
        }

        /// <summary>
        /// 设置指定的相机跟随目标
        /// </summary>
        /// <param name="name"></param>
        /// <param name="target"></param>
        public void SetFollowTarget(string cameraName, Transform target)
        {
            _cameraManager.SetFollowTarget(cameraName, target);
        }

        /// <summary>
        /// 设置指定的相机注视目标
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="transform"></param>
        public void SetLockAtTarget(string cameraName, Transform transform)
        {
            _cameraManager.SetLockAtTarget(cameraName, transform);
        }

        /// <summary>
        /// 获取子相机
        /// </summary>
        /// <param name="cameraName"></param>
        /// <returns></returns>
        public Camera GetSubCamera(string cameraName)
        {
            return _cameraManager.GetSubCamera(cameraName);
        }

        /// <summary>
        /// 获取所有拥有指定类型组件的实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Entity[] GetEntities<T>() where T : IComponent
        {
            Type type = typeof(T);
            uint[] entitys = _archetypes.GetHaveComponentEntityID(type);
            Entity[] entities = new Entity[entitys.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = _entitys.FindEntity(entitys[i]);
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
            return _entitys.FindEntity(id);
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <returns></returns>
        public Entity CreateEntity()
        {
            return _entitys.CreateEntity();
        }

        /// <summary>
        /// 销毁实体
        /// </summary>
        /// <param name="id"></param>
        public void DestroyEntity(uint id)
        {
            _entitys.DestroyEntity(id);
            _archetypes.RemoveEntityComponents(id);
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
            _entitys.Clear();
            _archetypes.Clear();
        }

        /// <summary>
        /// 注册逻辑系统
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RegisterGameLogicSystem(Type systemType, params object[] args)
        {
            _systems.RegisterGameLogicSystem(systemType, args);
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
            _systems.UnregisterGameLogicSystem(systemType);
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
            return _systems.GetSystem(type);
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent AddComponent(Entity entity, Type type)
        {
            return _archetypes.AddComponent(entity.id, type);
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent GetComponent(Entity entity, Type type)
        {
            return _archetypes.GetComponent(entity.id, type);
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveComponent(Entity entity, Type type)
        {
            _archetypes.RemoveComponent(entity.id, type);
        }

        /// <summary>
        /// 获取所有指定类型的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> AllOf<T>() where T : IComponent
        {
            return new ComponentEnumerable<T>(_archetypes.GetComponents(typeof(T)));
        }

        /// <summary>
        /// 获取实体上指定的组件
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Of<T>(Entity entity) where T : IComponent
        {
            return (T)_archetypes.GetComponent(entity.id, typeof(T));
        }

        /// <summary>
        /// 获取全局组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Single<T>() where T : ISingletonComponent
        {
            return (T)_archetypes.GetComponent(0, typeof(T));
        }
    }
}
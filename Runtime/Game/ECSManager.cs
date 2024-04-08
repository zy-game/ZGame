using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

namespace ZGame.Game
{
    /// <summary>
    /// ECS 管理器
    /// </summary>
    public partial class ECSManager : GameFrameworkModule
    {
        private ArchetypeManager _archetypeManager;
        private EntityManager entityManager;
        private SystemManager systemManager;

        /// <summary>
        /// 主世界
        /// </summary>
        public World curWorld { get; private set; }

        public override void OnAwake(params object[] args)
        {
            _archetypeManager = GameFrameworkFactory.Spawner<ArchetypeManager>();
            entityManager = GameFrameworkFactory.Spawner<EntityManager>();
            systemManager = GameFrameworkFactory.Spawner<SystemManager>();
            curWorld = World.Create("SYSTEM_DEFAULT_WORLD");
        }

        public override void Release()
        {
            GameFrameworkFactory.Release(_archetypeManager);
            GameFrameworkFactory.Release(systemManager);
            GameFrameworkFactory.Release(entityManager);
            GameFrameworkFactory.Release(curWorld);
            curWorld = null;
        }

        public override void Update()
        {
            systemManager.Update();
        }

        public override void FixedUpdate()
        {
            curWorld?.OnFixedUpdate();
            systemManager.FixedUpdate();
        }

        public override void LateUpdate()
        {
            curWorld.OnLateUpdate();
            systemManager.LateUpdate();
        }

        public override void OnDarwGizom()
        {
            curWorld?.OnDarwGizom();
            systemManager.OnDarwingGizmons();
        }

        public override void OnGUI()
        {
            curWorld?.OnGUI();
            systemManager.OnGUI();
            entityManager.OnGUI();
            _archetypeManager.OnGUI();
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
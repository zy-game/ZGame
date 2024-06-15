using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;
using FixMath.NET;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.Networking;
using ZGame.UI;
using ZGame.Resource;
using Space = BEPUphysics.Space;

namespace ZGame.Game
{
    public class World : IReference
    {
        class ComponentArraryList : IReference
        {
            public Type owner;
            public Dictionary<uint, IComponent> componentList;

            public void Release()
            {
                foreach (var VARIABLE in componentList.Values)
                {
                    RefPooled.Free(VARIABLE);
                }

                componentList.Clear();
                owner = null;
            }
        }

        private DateTime _startTime;
        private TimeSpan _fixDeletTime;
        private bool isEnable = true;
        private TimeSpan _lastFixedUpdateTime;
        private List<ISystem> systemList;
        private List<GameEntity> entityList;
        private List<ComponentArraryList> chunkList; //稀疏集


        /// <summary>
        /// 是否激活
        /// </summary>
        public bool active
        {
            get { return isEnable; }
        }

        /// <summary>
        /// 当前世界的时间
        /// </summary>
        public DateTime time
        {
            get { return DateTime.Now; }
        }

        /// <summary>
        /// 当前世界从启动到当前运行时长
        /// </summary>
        public TimeSpan realtimeSinceStartup
        {
            get { return (time - _startTime); }
        }

        /// <summary>
        /// 帧间隔时间
        /// </summary>
        public Fix64 fixedDeltaTime => _fixDeletTime.Milliseconds;

        /// <summary>
        /// 激活当前world
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public UniTask<Status> Awake(params object[] args)
        {
            SetFrameRate(100);
            _startTime = DateTime.Now;
            entityList = new();
            systemList = new();
            chunkList = new();
            _lastFixedUpdateTime = realtimeSinceStartup;
            return DoAwake(args);
        }

        /// <summary>
        /// 设置帧率
        /// </summary>
        /// <param name="rate"></param>
        public void SetFrameRate(int rate)
        {
            _fixDeletTime = TimeSpan.FromMilliseconds(rate);
        }

        /// <summary>
        /// 初始化当前world
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual UniTask<Status> DoAwake(params object[] args)
        {
            return UniTask.FromResult(Status.Success);
        }

        /// <summary>
        /// 当前<see cref="World"/>正在创建实体对象
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void DoCreateEntity(GameEntity entity)
        {
        }

        /// <summary>
        /// 当前<see cref="World"/>正在删除实体对象
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void DoDestroyEntity(GameEntity entity)
        {
        }

        /// <summary>
        /// 当前world正在绘制实体对象的显示框
        /// </summary>
        protected virtual void DoDarwGizom()
        {
        }

        /// <summary>
        /// 当前world正在轮询
        /// </summary>
        protected virtual void DoUpdate()
        {
        }

        /// <summary>
        /// 准备显示当前world所有实体对象
        /// </summary>
        protected virtual void DoEnable()
        {
        }

        /// <summary>
        /// 准备隐藏当前world所有实体对象
        /// </summary>
        protected virtual void DoDisable()
        {
        }

        /// <summary>
        /// 帧末轮询
        /// </summary>
        protected virtual void DoLateUpdate()
        {
        }

        /// <summary>
        /// 固定帧率轮询
        /// </summary>
        protected virtual void DoFixedUpdate()
        {
        }

        /// <summary>
        /// 显示当前<see cref="World"/>的所有物体
        /// </summary>
        public void Enable()
        {
            isEnable = true;
            DoEnable();
        }

        /// <summary>
        /// 隐藏当前<see cref="World"/>的所有物体
        /// </summary>
        public void Disable()
        {
            isEnable = false;
            DoDisable();
        }

        /// <summary>
        /// 轮询当前<see cref="World"/>
        /// </summary>
        public void Update()
        {
            if (isEnable is false)
            {
                return;
            }

            for (int i = systemList.Count - 1; i >= 0; i--)
            {
                if (systemList[i] is IUpdateSystem updateSystem)
                {
                    updateSystem.DoUpdate(this);
                }
            }

            DoUpdate();
        }

        public void LateUpdate()
        {
            if (isEnable is false)
            {
                return;
            }

            for (int i = systemList.Count - 1; i >= 0; i--)
            {
                if (systemList[i] is ILateUpdateSystem lateUpdateSystem)
                {
                    lateUpdateSystem.DoLateUpdate(this);
                }
            }

            DoLateUpdate();
        }

        public void FixedUpdate()
        {
            if (isEnable is false)
            {
                return;
            }

            while (realtimeSinceStartup - _lastFixedUpdateTime >= _fixDeletTime)
            {
                _lastFixedUpdateTime += _fixDeletTime;
                for (int i = systemList.Count - 1; i >= 0; i--)
                {
                    if (systemList[i] is IFixedUpdateSystem fixedUpdateSystem)
                    {
                        fixedUpdateSystem.DoFixedUpdate(this);
                    }
                }

                DoFixedUpdate();
            }
        }

        /// <summary>
        /// 绘制当前<see cref="World"/>的显示框
        /// </summary>
        public void OnDarwGizom()
        {
            if (isEnable is false)
            {
                return;
            }

            DoDarwGizom();
            for (int i = systemList.Count - 1; i >= 0; i--)
            {
                if (systemList[i] is IGizmoSystem gizmoSystem)
                {
                    gizmoSystem.DoDarwGizmo(this);
                }
            }
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal IComponent AddComponent(uint id, Type type)
        {
            if (typeof(IComponent).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException();
            }

            ComponentArraryList componentChunk = chunkList.Find(x => x.owner.IsAssignableFrom(type));
            if (componentChunk is null)
            {
                componentChunk = RefPooled.Alloc<ComponentArraryList>();
                componentChunk.owner = type;
                componentChunk.componentList = new();
                chunkList.Add(componentChunk);
            }

            if (componentChunk.componentList.ContainsKey(id))
            {
                return componentChunk.componentList[id];
            }

            componentChunk.componentList.Add(id, (IComponent)RefPooled.Alloc(type));
            return componentChunk.componentList[id];
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal IComponent GetComponent(uint id, Type type)
        {
            if (typeof(IComponent).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException();
            }

            ComponentArraryList componentChunk = chunkList.Find(x => x.owner.IsAssignableFrom(type));
            if (componentChunk is null)
            {
                return default;
            }

            if (componentChunk.componentList.ContainsKey(id))
            {
                return componentChunk.componentList[id];
            }

            return default;
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void RemoveComponent(uint id, Type type)
        {
            if (typeof(IComponent).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException();
            }

            ComponentArraryList componentChunk = chunkList.Find(x => x.owner.IsAssignableFrom(type));
            if (componentChunk is null)
            {
                return;
            }

            if (componentChunk.componentList.ContainsKey(id))
            {
                RefPooled.Free(componentChunk.componentList[id]);
                componentChunk.componentList.Remove(id);
            }
        }

        /// <summary>
        /// 获取所有指定类型的组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal IComponent[] GetComponents(Type type)
        {
            if (typeof(IComponent).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException();
            }

            ComponentArraryList componentChunk = chunkList.Find(x => x.owner.IsAssignableFrom(type));
            if (componentChunk is null)
            {
                return new IComponent[0];
            }

            return componentChunk.componentList.Values.ToArray();
        }

        /// <summary>
        /// 移除实体所有组件
        /// </summary>
        /// <param name="id"></param>
        private void RemoveComponents(uint id)
        {
            foreach (ComponentArraryList componentArraryList in chunkList)
            {
                if (componentArraryList.componentList.ContainsKey(id))
                {
                    RefPooled.Free(componentArraryList.componentList[id]);
                    componentArraryList.componentList.Remove(id);
                }
            }
        }

        internal bool TryGetComponent(uint id, Type type, out IComponent component)
        {
            component = default;
            foreach (ComponentArraryList componentArraryList in chunkList)
            {
                if (componentArraryList.componentList.ContainsKey(id))
                {
                    component = componentArraryList.componentList[id];
                }
            }

            return component != null;
        }

        /// <summary>
        /// 获取所有指定类型的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> AllOf<T>() where T : IComponent
        {
            return new ComponentEnumerable<T>(GetComponents(typeof(T)));
        }

        /// <summary>
        /// 获取实体上指定的组件
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Of<T>(GameEntity entity) where T : IComponent
        {
            return (T)GetComponent(entity.id, typeof(T));
        }

        /// <summary>
        /// 获取全局组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Single<T>() where T : ISingletonComponent
        {
            return (T)GetComponent(0, typeof(T));
        }

        /// <summary>
        /// 注册逻辑系统
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        public ISystem LoadSystem(Type systemType, params object[] args)
        {
            if (typeof(ISystem).IsAssignableFrom(systemType) is false)
            {
                throw new NotImplementedException(nameof(ISystem));
            }

            UnloadSystem(systemType);
            ISystem system = (ISystem)RefPooled.Alloc(systemType);
            system.DoAwake(this, args);
            systemList.Add(system);
            systemList.Sort((x, y) => x.priority.CompareTo(y.priority));
            return system;
        }

        /// <summary>
        /// 注册逻辑系统
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        public T LoadSystem<T>(params object[] args) where T : ISystem
        {
            return (T)LoadSystem(typeof(T), args);
        }

        /// <summary>
        /// 卸载逻辑系统
        /// </summary>
        /// <param name="systemType"></param>
        public void UnloadSystem(Type systemType)
        {
            if (typeof(ISystem).IsAssignableFrom(systemType) is false)
            {
                throw new NotImplementedException(nameof(ISystem));
            }

            systemList.RemoveAll(x => x.GetType() == systemType);
        }

        /// <summary>
        /// 卸载逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnloadSystem<T>() where T : ISystem
        {
            UnloadSystem(typeof(T));
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
            if (typeof(ISystem).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(ISystem));
            }

            return systemList.Find(x => type.IsAssignableFrom(x.GetType()));
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <returns></returns>
        public GameEntity CreateEntity(uint id = 0)
        {
            GameEntity entity = GameEntity.Create(this, id);
            entityList.Add(entity);
            DoCreateEntity(entity);
            return entity;
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GameEntity GetEntity(uint id)
        {
            return entityList.Find(x => x.id == id);
        }

        /// <summary>
        /// 获取所有实体对象
        /// </summary>
        /// <returns></returns>
        public GameEntity[] GetEntities()
        {
            return entityList.ToArray();
        }

        /// <summary>
        /// 销毁实体
        /// </summary>
        /// <param name="id"></param>
        public void DestroyEntity(uint id)
        {
            var entity = entityList.Find(x => x.id == id);
            if (entity is null)
            {
                return;
            }

            DoDestroyEntity(entity);
            entityList.Remove(entity);
            RefPooled.Free(entity);
            RemoveComponents(id);
        }

        /// <summary>
        /// 销毁实体
        /// </summary>
        /// <param name="entity"></param>
        public void DestroyEntity(GameEntity entity)
        {
            DestroyEntity(entity.id);
        }


        /// <summary>
        /// 清理所有实体对象
        /// </summary>
        public void ClearEntity()
        {
            foreach (var entity in entityList)
            {
                DoDestroyEntity(entity);
                RemoveComponents(entity.id);
                RefPooled.Free(entity);
            }

            entityList.Clear();
        }


        public virtual void Release()
        {
            entityList.ForEach(RefPooled.Free);
            entityList.Clear();
            systemList.ForEach(RefPooled.Free);
            systemList.Clear();
            chunkList.ForEach(RefPooled.Free);
            chunkList.Clear();
        }
    }
}
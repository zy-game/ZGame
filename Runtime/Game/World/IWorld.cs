using System;
using System.Collections.Generic;

namespace ZEngine.Game
{
    public class MapData
    {
    }

    /// <summary>
    /// 世界
    /// </summary>
    public interface IWorld : IDisposable
    {
        /// <summary>
        /// 世界名
        /// </summary>
        string name { get; }

        void FixedUpdate();

        /// <summary>
        /// 创建实体对象
        /// </summary>
        /// <returns></returns>
        IEntity CreateEntity();

        /// <summary>
        /// 删除实体对象
        /// </summary>
        /// <param name="id"></param>
        void DestroyEntity(int id);

        /// <summary>
        /// 查找实体对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEntity Find(int id);

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="type"></param>
        void DestroyComponent(int id, Type type);

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEntityComponent GetComponent(int id, Type type);

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEntityComponent AddComponent(int id, Type type);

        /// <summary>
        /// 获取实体所有组件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEntityComponent[] GetComponents(int id);

        /// <summary>
        /// 获取相同类型的组件列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEntityComponent[] GetComponents(Type type);

        /// <summary>
        /// 加载游戏逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void LoadGameLogicSystem<T>() where T : IGameLogicSystem
        {
            LoadGameLogicSystem(typeof(T));
        }

        /// <summary>
        /// 加载游戏逻辑系统
        /// </summary>
        /// <param name="logicType"></param>
        void LoadGameLogicSystem(Type logicType);

        /// <summary>
        /// 卸载游戏逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void UnloadGameLogicSystem<T>() where T : IGameLogicSystem
        {
            UnloadGameLogicSystem(typeof(T));
        }

        /// <summary>
        /// 卸载游戏逻辑系统
        /// </summary>
        /// <param name="logicType"></param>
        void UnloadGameLogicSystem(Type logicType);

        /// <summary>
        /// 创建游戏世界
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static IWorld Create(string name)
        {
            World world = Activator.CreateInstance<World>();
            world.Init(name);
            return world;
        }

        class World : IWorld
        {
            public string name { get; set; }

            private List<IEntity> entities;
            private List<EntityContext> contexts;
            private List<IGameLogicSystem> systems;

            class EntityContext : IDisposable
            {
                public int id;
                public List<IEntityComponent> components;

                public EntityContext()
                {
                    components = new List<IEntityComponent>();
                }

                public IEntityComponent Add(Type type)
                {
                    IEntityComponent component = Find(type);
                    if (component is not null)
                    {
                        return component;
                    }

                    component = (IEntityComponent)Activator.CreateInstance(type);
                    components.Add(component);
                    return component;
                }

                public void Remove(Type type)
                {
                    IEntityComponent component = Find(type);
                    if (component is null)
                    {
                        return;
                    }

                    components.Remove(component);
                }

                public IEntityComponent Find(Type type)
                {
                    return components.Find(x => x.GetType() == type);
                }

                public void Dispose()
                {
                    components.ForEach(x => x.Dispose());
                    components.Clear();
                    id = 0;
                }
            }


            public void Init(string name)
            {
                this.name = name;
                entities = new List<IEntity>();
                systems = new List<IGameLogicSystem>();
                contexts = new List<EntityContext>();
            }

            
            public void FixedUpdate()
            {
                if (systems is null || systems.Count is 0)
                {
                    return;
                }

                for (int i = 0; i < systems.Count; i++)
                {
                    systems[i].OnUpdate();
                }
            }

            public void Dispose()
            {
                systems.ForEach(x => x.Dispose());
                contexts.ForEach(x => x.Dispose());
                entities.ForEach(x => x.Dispose());
                systems.Clear();
                contexts.Clear();
                entities.Clear();
                name = string.Empty;
            }

            public IEntity CreateEntity()
            {
                IEntity entity = IEntity.Create();
                entities.Add(entity);
                return entity;
            }

            public void DestroyEntity(int id)
            {
                IEntity entity = Find(id);
                if (entity is null)
                {
                    return;
                }

                entities.Remove(entity);
                EntityContext context = contexts.Find(x => x.id == id);
                if (context is null)
                {
                    return;
                }

                contexts.Remove(context);
                context.Dispose();
            }

            public IEntity Find(int id)
            {
                return entities.Find(x => x.id == id);
            }

            public void DestroyComponent(int id, Type type)
            {
                EntityContext context = contexts.Find(x => x.id == id);
                if (context is null)
                {
                    return;
                }

                context.Remove(type);
            }

            public IEntityComponent GetComponent(int id, Type type)
            {
                EntityContext context = contexts.Find(x => x.id == id);
                if (context is null)
                {
                    return default;
                }

                return context.Find(type);
            }

            public IEntityComponent AddComponent(int id, Type type)
            {
                EntityContext context = contexts.Find(x => x.id == id);
                if (context is null)
                {
                    return default;
                }

                return context.Add(type);
            }

            public IEntityComponent[] GetComponents(int id)
            {
                EntityContext context = contexts.Find(x => x.id == id);
                if (context is null)
                {
                    return default;
                }

                return context.components.ToArray();
            }

            public IEntityComponent[] GetComponents(Type type)
            {
                List<IEntityComponent> components = new List<IEntityComponent>();
                for (int i = 0; i < contexts.Count; i++)
                {
                    IEntityComponent component = contexts[i].Find(type);
                    if (component is null)
                    {
                        continue;
                    }

                    components.Add(component);
                }

                return components.ToArray();
            }

            public void LoadGameLogicSystem(Type logicType)
            {
                IGameLogicSystem gameLogicSystem = systems.Find(x => x.GetType() == logicType);
                if (gameLogicSystem is not null)
                {
                    return;
                }

                gameLogicSystem = (IGameLogicSystem)Activator.CreateInstance(logicType);
                gameLogicSystem.OnCreate();
                systems.Add(gameLogicSystem);
            }

            public void UnloadGameLogicSystem(Type logicType)
            {
                IGameLogicSystem gameLogicSystem = systems.Find(x => x.GetType() == logicType);
                if (gameLogicSystem is null)
                {
                    return;
                }

                gameLogicSystem.OnDestroy();
                systems.Remove(gameLogicSystem);
            }
        }
    }
}
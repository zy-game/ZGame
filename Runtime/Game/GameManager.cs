using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using ZEngine.Network;

namespace ZEngine.Game
{
    public class WorldOptions
    {
        public string name;
        public MapData mapData;
    }

    /// <summary>
    /// 游戏管理器
    /// </summary>
    class GameManager : Single<GameManager>
    {
        private List<IWorld> worlds = new List<IWorld>();

        /// <summary>
        /// 当前正在使用的world
        /// </summary>
        public IWorld current
        {
            get { return default; }
        }

        /// <summary>
        /// 创建world
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IWorld OpenWorld(WorldOptions options)
        {
            IWorld world = Find(options.name);
            if (world is not null)
            {
                return world;
            }

            world = IWorld.Create(options.name);
            worlds.Add(world);
            return world;
        }

        /// <summary>
        /// 查找world
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IWorld Find(string name)
        {
            return worlds.Find(x => x.name == name);
        }

        /// <summary>
        /// 关闭world
        /// </summary>
        /// <param name="name"></param>
        public void CloseWorld(string name)
        {
            IWorld world = Find(name);
            if (world is null)
            {
                return;
            }

            worlds.Remove(world);
            world.Dispose();
        }

        /// <summary>
        /// 加载游戏模块
        /// </summary>
        /// <param name="gameEntryOptions">游戏入口配置</param>
        /// <returns></returns>
        public IGameModuleLoaderExecuteHandle LoadGameModule(GameEntryOptions gameEntryOptions)
        {
            IGameModuleLoaderExecuteHandle gameModuleLoaderExecuteHandle = IGameModuleLoaderExecuteHandle.Create(gameEntryOptions);
            gameModuleLoaderExecuteHandle.Execute();
            return gameModuleLoaderExecuteHandle;
        }

        /// <summary>
        /// 创建实体对象
        /// </summary>
        /// <returns></returns>
        public IEntity CreateEntity()
        {
            return current.CreateEntity();
        }

        /// <summary>
        /// 删除实体对象
        /// </summary>
        /// <param name="id"></param>
        public void DestroyEntity(int id)
        {
            current.DestroyEntity(id);
        }

        /// <summary>
        /// 查找实体对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEntity Find(int id)
        {
            return current.Find(id);
        }

        /// <summary>
        /// 获取实体所有组件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEntityComponent[] GetComponents(int id)
        {
            return current.GetComponents(id);
        }

        /// <summary>
        /// 获取相同类型的组件列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEntityComponent[] GetComponents(Type type)
        {
            return current.GetComponents(type);
        }

        /// <summary>
        /// 加载游戏逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void LoadGameLogicSystem<T>() where T : IGameLogicSystem
        {
            LoadGameLogicSystem(typeof(T));
        }

        /// <summary>
        /// 加载游戏逻辑系统
        /// </summary>
        /// <param name="logicType"></param>
        public void LoadGameLogicSystem(Type logicType)
        {
            current.LoadGameLogicSystem(logicType);
        }

        /// <summary>
        /// 卸载游戏逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnloadGameLogicSystem<T>() where T : IGameLogicSystem
        {
            UnloadGameLogicSystem(typeof(T));
        }

        /// <summary>
        /// 卸载游戏逻辑系统
        /// </summary>
        /// <param name="logicType"></param>
        public void UnloadGameLogicSystem(Type logicType)
        {
            current.UnloadGameLogicSystem(logicType);
        }
    }
}
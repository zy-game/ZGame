using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HybridCLR;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using ZEngine.Network;
using ZEngine.Resource;

namespace ZEngine.Game
{
    public interface IGameModuleLoaderExecuteHandle : IExecuteHandle
    {
        Assembly assembly { get; }
        GameEntryOptions gameEntryOptions { get; }


        internal static IGameModuleLoaderExecuteHandle Create(GameEntryOptions gameEntryOptions)
        {
            InternalGameModuleLoaderExecuteHandle internalGameModuleLoaderExecuteHandle = Engine.Class.Loader<InternalGameModuleLoaderExecuteHandle>();
            internalGameModuleLoaderExecuteHandle.Execute(gameEntryOptions);
            return internalGameModuleLoaderExecuteHandle;
        }

        class InternalGameModuleLoaderExecuteHandle : AbstractExecuteHandle, IGameModuleLoaderExecuteHandle
        {
            public Assembly assembly { get; set; }
            public GameEntryOptions gameEntryOptions { get; set; }

            protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
            {
                gameEntryOptions = (GameEntryOptions)paramsList[0];
                IRequestAssetExecute<TextAsset> requestAssetExecuteResult = default;
                if (gameEntryOptions.aotList is not null && gameEntryOptions.aotList.Count > 0)
                {
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    foreach (var item in gameEntryOptions.aotList)
                    {
                        requestAssetExecuteResult = Engine.Resource.LoadAsset<TextAsset>(item + ".bytes");
                        if (requestAssetExecuteResult.asset == null)
                        {
                            yield break;
                        }

                        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(requestAssetExecuteResult.asset.bytes, mode);
                        Debug.Log($"LoadMetadataForAOTAssembly:{item}. mode:{mode} ret:{err}");
                    }
                }

                requestAssetExecuteResult = Engine.Resource.LoadAsset<TextAsset>(gameEntryOptions.dllName);
                if (requestAssetExecuteResult.asset == null)
                {
                    status = Status.Failed;
                    yield break;
                }

                try
                {
                    assembly = Assembly.Load(requestAssetExecuteResult.asset.bytes);
                    Type entryType = assembly.GetType(gameEntryOptions.methodName);
                    if (entryType is null)
                    {
                        Engine.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                        status = Status.Failed;
                        yield break;
                    }

                    string methodName = gameEntryOptions.methodName.Substring(gameEntryOptions.methodName.LastIndexOf('.') + 1);
                    MethodInfo entry = entryType.GetMethod(methodName);
                    if (entry is null)
                    {
                        Engine.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                        status = Status.Failed;
                        yield break;
                    }

                    entry.Invoke(null, gameEntryOptions.paramsList?.ToArray());
                    status = Status.Success;
                }
                catch (Exception e)
                {
                    status = Status.Failed;
                    Engine.Console.Error(e);
                    Engine.Window.MsgBox("Loading Game Fail", Engine.Custom.Quit);
                }
            }
        }
    }

    /// <summary>
    /// 游戏管理器
    /// </summary>
    public class GameManager : Single<GameManager>
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
        public IWorld OpenWorld(string name)
        {
            IWorld world = Find(name);
            if (world is not null)
            {
                return world;
            }

            world = IWorld.Create(name);
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
            Engine.Class.Release(world);
        }

        /// <summary>
        /// 加载游戏模块
        /// </summary>
        /// <param name="gameEntryOptions">游戏入口配置</param>
        /// <returns></returns>
        public IGameModuleLoaderExecuteHandle LoadGameModule(GameEntryOptions gameEntryOptions)
        {
            return IGameModuleLoaderExecuteHandle.Create(gameEntryOptions);
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
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
    public class WorldOptions
    {
        public string name;
        public MapData mapData;
    }

    public interface IGameLogicLoadResult : IScheduleHandle<IGameLogicLoadResult>
    {
        Assembly assembly { get; }
        GameEntryOptions gameEntryOptions { get; }

        public static IGameLogicLoadResult Create(GameEntryOptions gameEntryOptions)
        {
            LogicModuleLoadResult logicModuleLoadResult = Activator.CreateInstance<LogicModuleLoadResult>();
            logicModuleLoadResult.gameEntryOptions = gameEntryOptions;
            return logicModuleLoadResult;
        }

        class LogicModuleLoadResult : IGameLogicLoadResult
        {
            public Status status { get; set; }

            public Assembly assembly { get; set; }
            public GameEntryOptions gameEntryOptions { get; set; }
            public IGameLogicLoadResult result => this;
            private ISubscriber subscriber;

            public void Dispose()
            {
                gameEntryOptions = null;
                assembly = null;
                status = Status.None;
                GC.SuppressFinalize(this);
            }

            public void Execute(params object[] args)
            {
                if (status is not Status.None)
                {
                    return;
                }

                status = Status.Execute;
                StartLoadGameLogicAssembly().StartCoroutine(OnComplate);
            }

            private void OnComplate()
            {
                if (subscriber is not null)
                {
                    subscriber.Execute(this);
                }

            }


            public void Subscribe(ISubscriber subscriber)
            {
                if (this.subscriber is null)
                {
                    this.subscriber = subscriber;
                    return;
                }

                this.subscriber.Merge(subscriber);
            }

            IEnumerator StartLoadGameLogicAssembly()
            {
                if (gameEntryOptions is null || gameEntryOptions.methodName.IsNullOrEmpty() || gameEntryOptions.isOn == Switch.Off)
                {
                    Engine.Console.Error("模块入口参数错误");
                    yield break;
                }

                yield return LoadAOTDll();
                yield return LoadLogicAssembly();
            }


            public IEnumerator LoadLogicAssembly()
            {
                IRequestAssetObjectSchedule<TextAsset> requestAssetObjectSchedule = default;
                requestAssetObjectSchedule = Engine.Resource.LoadAsset<TextAsset>(gameEntryOptions.dllName);
                if (requestAssetObjectSchedule.result == null)
                {
                    status = Status.Failed;
                    yield break;
                }

                try
                {
                    assembly = Assembly.Load(requestAssetObjectSchedule.result.bytes);
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
                    Engine.Console.Error(e);
                }
            }

            public IEnumerator LoadAOTDll()
            {
                IRequestAssetObjectSchedule<TextAsset> requestAssetObjectSchedule = default;
                if (gameEntryOptions.aotList is not null && gameEntryOptions.aotList.Count > 0)
                {
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    foreach (var item in gameEntryOptions.aotList)
                    {
                        requestAssetObjectSchedule = Engine.Resource.LoadAsset<TextAsset>(item + ".bytes");
                        if (requestAssetObjectSchedule.result == null)
                        {
                            status = Status.Failed;
                            yield break;
                        }

                        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(requestAssetObjectSchedule.result.bytes, mode);
                        Debug.Log($"LoadMetadataForAOTAssembly:{item}. mode:{mode} ret:{err}");
                    }
                }
            }
        }
    }


    /// <summary>
    /// 游戏管理器
    /// </summary>
    class GameManager : Singleton<GameManager>
    {
        private List<IWorld> worlds = new List<IWorld>();

        /// <summary>
        /// 当前正在使用的world
        /// </summary>
        public IWorld current
        {
            get { return default; }
        }

        protected override void OnFixedUpdate()
        {
            if (current is null)
            {
                return;
            }

            current.FixedUpdate();
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
        public IComponent[] GetComponents(int id)
        {
            return current.GetComponents(id);
        }

        /// <summary>
        /// 获取相同类型的组件列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IComponent[] GetComponents(Type type)
        {
            return current.GetComponents(type);
        }

        /// <summary>
        /// 加载游戏逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void LoadGameLogicSystem<T>() where T : ILogicSystem
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
        public void UnloadGameLogicSystem<T>() where T : ILogicSystem
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

        public IGameLogicLoadResult LoadGameLogicAssembly(GameEntryOptions gameEntryOptions)
        {
            IGameLogicLoadResult gameLogicLoadResult = IGameLogicLoadResult.Create(gameEntryOptions);
            gameLogicLoadResult.Execute();
            return gameLogicLoadResult;
        }
    }
}
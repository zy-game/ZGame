using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using ZEngine.Network;
using ZEngine.Resource;

namespace ZEngine.Game
{
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
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix == Switch.Off || HotfixOptions.instance.useScript == Switch.Off)
                {
                    RPCHandle.instance.RegisterMessageType(FindAllType<IMessaged>());
                    status = Status.Success;
                    yield break;
                }
#endif
                if (gameEntryOptions is null || gameEntryOptions.methodName.IsNullOrEmpty() || gameEntryOptions.isOn == Switch.Off)
                {
                    Engine.Console.Error("模块入口参数错误");
                    status = Status.Failed;
                    yield break;
                }

                yield return LoadAOTDll();
                yield return LoadLogicAssembly();
                RPCHandle.instance.RegisterMessageType(FindAllType<IMessaged>());
                status = Status.Success;
            }

            private Type[] FindAllType<T>()
            {
                List<Type> result = new List<Type>();
                Type source = typeof(T);
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix == Switch.Off || HotfixOptions.instance.useScript == Switch.Off)
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var VARIABLE in a.GetTypes())
                        {
                            if (source.IsAssignableFrom(VARIABLE) && VARIABLE.IsInterface == false && VARIABLE.IsAbstract == false)
                            {
                                result.Add(VARIABLE);
                            }
                        }
                    }
                }
#endif
                if (assembly is not null)
                {
                    foreach (var VARIABLE in assembly.GetTypes())
                    {
                        if (source.IsAssignableFrom(VARIABLE) && VARIABLE.IsInterface == false && VARIABLE.IsAbstract == false)
                        {
                            result.Add(VARIABLE);
                        }
                    }
                }

                return result.ToArray();
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
}
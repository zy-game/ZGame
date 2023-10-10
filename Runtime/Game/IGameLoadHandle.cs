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
    public interface IGameLoadHandle : IScheduleHandle<IGameLoadHandle>
    {
        Assembly assembly { get; }
        GameEntryOptions gameEntryOptions { get; }

        public static IGameLoadHandle Create(GameEntryOptions gameEntryOptions)
        {
            ModuleLoadHandle moduleLoadHandle = Activator.CreateInstance<ModuleLoadHandle>();
            moduleLoadHandle.gameEntryOptions = gameEntryOptions;
            return moduleLoadHandle;
        }

        class ModuleLoadHandle : IGameLoadHandle
        {
            public Status status { get; set; }

            public Assembly assembly { get; set; }
            public GameEntryOptions gameEntryOptions { get; set; }
            public IGameLoadHandle result => this;
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

                status = assembly == null ? Status.Failed : Status.Success;
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
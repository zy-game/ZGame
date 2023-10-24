using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Game
{
    public sealed class DllImport : Singleton<DllImport>
    {
        private HashSet<string> aotList = new HashSet<string>();

        public async UniTask<IDllImportReslt> Import(GameEntryOptions gameEntryOptions)
        {
#if UNITY_EDITOR
            if (HotfixOptions.instance.useHotfix == Switch.Off || HotfixOptions.instance.useScript == Switch.Off)
            {
                return IDllImportReslt.Create(Status.Success, AppDomain.CurrentDomain.Find(gameEntryOptions.dllName), gameEntryOptions);
            }
#endif
            if (gameEntryOptions is null || gameEntryOptions.methodName.IsNullOrEmpty() || gameEntryOptions.isOn == Switch.Off)
            {
                ZGame.Console.Error("模块入口参数错误");
                return IDllImportReslt.Failur(gameEntryOptions);
            }

            IRequestResourceObjectResult requestResourceObjectResult = await ZGame.Resource.LoadAssetAsync(gameEntryOptions.dllName);
            if (requestResourceObjectResult.status is not Status.Success)
            {
                ZGame.Console.Error("加载DLL资源失败");
                return IDllImportReslt.Failur(gameEntryOptions);
            }

            try
            {
                Assembly assembly = Assembly.Load(requestResourceObjectResult.GetObject<TextAsset>().bytes);
                if (gameEntryOptions.aotList is not null && gameEntryOptions.aotList.Count > 0)
                {
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    foreach (var item in gameEntryOptions.aotList)
                    {
                        if (aotList.Contains(item))
                        {
                            continue;
                        }

                        requestResourceObjectResult = await ZGame.Resource.LoadAssetAsync(item + ".bytes");
                        if (requestResourceObjectResult.result == null)
                        {
                            ZGame.Console.Error("加载AOT补元数据资源失败");
                            return IDllImportReslt.Failur(gameEntryOptions);
                        }

                        aotList.Add(item);
                        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(requestResourceObjectResult.GetObject<TextAsset>().bytes, mode);
                        Debug.Log($"LoadMetadataForAOTAssembly:{item}. mode:{mode} ret:{err}");
                    }
                }

                Type entryType = assembly.GetType(gameEntryOptions.methodName);
                if (entryType is null)
                {
                    ZGame.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                    return IDllImportReslt.Failur(gameEntryOptions);
                }

                string methodName = gameEntryOptions.methodName.Substring(gameEntryOptions.methodName.LastIndexOf('.') + 1);
                MethodInfo entry = entryType.GetMethod(methodName);
                if (entry is null)
                {
                    ZGame.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                    return IDllImportReslt.Failur(gameEntryOptions);
                }

                entry.Invoke(null, gameEntryOptions.paramsList?.ToArray());
                return IDllImportReslt.Create(Status.Success, assembly, gameEntryOptions);
            }
            catch (Exception e)
            {
                ZGame.Console.Error(e);
                return IDllImportReslt.Failur(gameEntryOptions);
            }
        }
    }

    public interface IDllImportReslt : IDisposable
    {
        Status status { get; }
        Assembly assembly { get; }
        GameEntryOptions options { get; }

        public static IDllImportReslt Create(Status status, Assembly assembly, GameEntryOptions options)
        {
            return new DllImportResult()
            {
                status = status,
                assembly = assembly,
                options = options
            };
        }

        public static IDllImportReslt Failur(GameEntryOptions options)
        {
            return new DllImportResult()
            {
                status = Status.Failed,
                options = options
            };
        }

        class DllImportResult : IDllImportReslt
        {
            public void Dispose()
            {
                status = Status.None;
                assembly = null;
                options = null;
            }

            public Status status { get; set; }
            public Assembly assembly { get; set; }
            public GameEntryOptions options { get; set; }
        }
    }
}
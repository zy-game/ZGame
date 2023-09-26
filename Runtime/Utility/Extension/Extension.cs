using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using UnityEngine.Events;
using ZEngine;
using ZEngine.Resource;

public static class Extension
{
    public static IExecuteHandle<Assembly> LoadAssembly(this GameEntryOptions gameEntryOptions)
    {
        GameExecuteHandle<Assembly> execute = IExecuteHandle<Assembly>.Create();
        execute.SetMethod(StartLoadGameLogicAssembly());
        execute.Execute();

        IEnumerator StartLoadGameLogicAssembly()
        {
            if (gameEntryOptions is null || gameEntryOptions.methodName.IsNullOrEmpty() || gameEntryOptions.isOn == Switch.Off)
            {
                Engine.Console.Error("模块入口参数错误");
                yield break;
            }

            yield return LoadAOTDll(gameEntryOptions, execute);
            yield return LoadLogicAssembly(gameEntryOptions, execute);
        }

        return execute;
    }

    public static IEnumerator LoadLogicAssembly(GameEntryOptions gameEntryOptions, GameExecuteHandle<Assembly> execute)
    {
        if (execute.status is not Status.Execute)
        {
            yield break;
        }

        IRequestAssetExecute<TextAsset> requestAssetExecuteResult = default;
        requestAssetExecuteResult = Engine.Resource.LoadAsset<TextAsset>(gameEntryOptions.dllName);
        if (requestAssetExecuteResult.asset == null)
        {
            execute.status = Status.Failed;
            yield break;
        }

        try
        {
            Assembly assembly = Assembly.Load(requestAssetExecuteResult.asset.bytes);
            Type entryType = assembly.GetType(gameEntryOptions.methodName);
            if (entryType is null)
            {
                Engine.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                execute.status = Status.Failed;
                yield break;
            }

            string methodName = gameEntryOptions.methodName.Substring(gameEntryOptions.methodName.LastIndexOf('.') + 1);
            MethodInfo entry = entryType.GetMethod(methodName);
            if (entry is null)
            {
                Engine.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                execute.status = Status.Failed;
                yield break;
            }

            entry.Invoke(null, gameEntryOptions.paramsList?.ToArray());
            execute.status = Status.Success;
        }
        catch (Exception e)
        {
            Engine.Console.Error(e);
        }
    }

    public static IEnumerator LoadAOTDll(GameEntryOptions gameEntryOptions, GameExecuteHandle<Assembly> execute)
    {
        IRequestAssetExecute<TextAsset> requestAssetExecuteResult = default;
        if (gameEntryOptions.aotList is not null && gameEntryOptions.aotList.Count > 0)
        {
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var item in gameEntryOptions.aotList)
            {
                requestAssetExecuteResult = Engine.Resource.LoadAsset<TextAsset>(item + ".bytes");
                if (requestAssetExecuteResult.asset == null)
                {
                    execute.status = Status.Failed;
                    yield break;
                }

                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(requestAssetExecuteResult.asset.bytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{item}. mode:{mode} ret:{err}");
            }
        }
    }

    public static T TryGetComponent<T>(this GameObject gameObject) where T : Component
    {
        T linker = gameObject.GetComponent<T>();
        if (linker == null)
        {
            linker = gameObject.AddComponent<T>();
        }

        return linker;
    }

    public static void OnDestroyEvent(this GameObject gameObject, UnityAction action)
    {
        gameObject.TryGetComponent<UnityBehaviour>().OnDestroyGameObject(action);
    }

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static Coroutine StartCoroutine(this IDisposable reference, IEnumerator enumerator)
    {
        return StartCoroutine(enumerator);
    }

    public static Coroutine StartCoroutine(this IEnumerator enumerator)
    {
        IEnumerator Running()
        {
            yield return new WaitForEndOfFrame();
            yield return enumerator;
        }

        return UnityBehaviour.instance.StartCoroutine(Running());
    }

    public static Coroutine StartCoroutine(this IEnumerator enumerator, Action complete)
    {
        IEnumerator Running()
        {
            yield return new WaitForEndOfFrame();
            yield return enumerator;
            yield return new WaitForEndOfFrame();
            complete();
        }

        return UnityBehaviour.instance.StartCoroutine(Running());
    }

    public static void StopAll()
    {
        UnityBehaviour.instance.StopAllCoroutines();
    }

    public static void StopCoroutine(this Coroutine coroutine)
    {
        UnityBehaviour.instance.StopCoroutine(coroutine);
    }

    public static void SetParent(this GameObject gameObject, GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        if (gameObject == null || parent == null)
        {
            return;
        }

        gameObject.transform.SetParent(parent.transform);
        gameObject.transform.position = position;
        gameObject.transform.rotation = Quaternion.Euler(rotation);
        gameObject.transform.localScale = scale;
    }
}
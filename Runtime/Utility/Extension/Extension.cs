using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;
using ZEngine;
using ZEngine.Resource;
using Object = UnityEngine.Object;

public static class Extension
{
    /// <summary>
    /// 不安全的等待一个调度器
    /// </summary>
    /// <param name="schecule"></param>
    /// <returns></returns>
    public static Task UnsafeWait(this IScheduleHandle schecule)
    {
        return Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                if (schecule.status == Status.Failed || schecule.status == Status.Success)
                {
                    return;
                }

                await Task.Delay(100);
            }
        });
    }

    /// <summary>
    /// 等待一个调度器执行完成
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public static WaitFor WaitTo(this IScheduleHandle schedule)
    {
        return WaitFor.Create(() => schedule.status == Status.Failed || schedule.status == Status.Success);
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

    public static void OnDestroyEvent(this GameObject gameObject, UnityAction action)
    {
        gameObject.TryGetComponent<UnityBehaviour>().OnDestroyGameObject(action);
    }

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
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
}
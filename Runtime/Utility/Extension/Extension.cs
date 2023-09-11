using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;

public static class Extension
{
    public static void ReleaseAll<T>(this List<T> list)
    {
        if (typeof(IReference).IsAssignableFrom(typeof(T)) is false)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            Engine.Class.Release((IReference)list[i]);
        }

        list.Clear();
    }

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static Coroutine StartCoroutine(this IReference reference, IEnumerator enumerator)
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

        return UnityEventArgs.UnityEventHandle.instance.StartCoroutine(Running());
    }

    public static Coroutine StartCoroutine(this IEnumerator enumerator, Action complete)
    {
        IEnumerator Running()
        {
            yield return enumerator;
            yield return new WaitForEndOfFrame();
            complete();
        }

        return UnityEventArgs.UnityEventHandle.instance.StartCoroutine(Running());
    }

    public static void StopAll()
    {
        UnityEventArgs.UnityEventHandle.instance.StopAllCoroutines();
    }

    public static void StopCoroutine(this Coroutine coroutine)
    {
        UnityEventArgs.UnityEventHandle.instance.StopCoroutine(coroutine);
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
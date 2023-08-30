using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;

public static class Extension
{
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
}
using System.Collections;
using UnityEngine;

namespace ZEngine
{
    public static class EnumeratorExtension
    {
        public static Coroutine StartCoroutine(this IEnumerator enumerator)
        {
            IEnumerator Running()
            {
                yield return new WaitForEndOfFrame();
                yield return enumerator;
            }

            return Behaviour.instance.StartCoroutine(Running());
        }

        public static void StopAll()
        {
            Behaviour.instance.StopAllCoroutines();
        }

        public static void StopCoroutine(this Coroutine coroutine)
        {
            Behaviour.instance.StopCoroutine(coroutine);
        }
    }
}
using System.Collections;
using UnityEngine;

namespace ZGame.Thread
{
    public class CoroutineManager : GameFrameworkModule
    {
        private CoroutineManagerHandle _handle;

        class CoroutineManagerHandle : MonoBehaviour
        {
        }

        public override void OnAwake()
        {
            _handle = new GameObject("CoroutineManager").AddComponent<CoroutineManagerHandle>();
            _handle.gameObject.SetParent(null, Vector3.zero, Vector3.zero, Vector3.one);
            GameObject.DontDestroyOnLoad(_handle.gameObject);
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return _handle.StartCoroutine(enumerator);
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            _handle.StopCoroutine(coroutine);
        }
    }
}
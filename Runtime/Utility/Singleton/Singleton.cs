using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ZGame
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        public static T instance => GetInstance();
        private static T _instance;
        private static BevaviourScriptable singleton;
        private static GameObject manager;
        public GameObject gameObject { get; private set; }

        private static T GetInstance()
        {
            if (_instance is not null)
            {
                return _instance;
            }

            if (manager == null)
            {
                manager = GameObject.Find("Manager") ?? new GameObject("Manager");
                GameObject.DontDestroyOnLoad(manager);
            }

            _instance = Activator.CreateInstance<T>();
            _instance.gameObject = new GameObject(typeof(T).Name);
            _instance.gameObject.transform.SetParent(manager.transform);
            singleton = _instance.gameObject.AddComponent<BevaviourScriptable>();
            GameObject.DontDestroyOnLoad(_instance.gameObject);
            singleton.update.AddListener(_instance.OnUpdate);
            singleton.fixedUpdate.AddListener(_instance.OnFixedUpdate);
            singleton.lateUpdate.AddListener(_instance.OnLateUpdate);
            singleton.onGUI.AddListener(_instance.OnGUI);
            singleton.onApplicationQuit.AddListener(_instance.OnApplicationQuit);
            singleton.onApplicationPause.AddListener(_instance.OnApplicationPause);
            singleton.onApplicationFocus.AddListener(_instance.OnApplicationFocus);
            singleton.onDestroy.AddListener(_instance.OnDestroy);
            _instance.OnAwake();
            return _instance;
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return singleton?.StartCoroutine(enumerator);
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            singleton?.StopCoroutine(coroutine);
        }

        public void StopAllCoroutine()
        {
            singleton?.StopAllCoroutines();
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnFixedUpdate()
        {
        }

        protected virtual void OnLateUpdate()
        {
        }

        protected virtual void OnGUI()
        {
        }

        protected virtual void OnApplicationQuit()
        {
        }

        protected virtual void OnApplicationPause(bool pause)
        {
        }

        protected virtual void OnApplicationFocus(bool focus)
        {
        }
    }
}
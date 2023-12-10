using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ZGame
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        public static T instance => SinglePipeline.GetInstance();


        class SinglePipeline
        {
            private static T _entity;
            public static UnitySingleton singleton;

            public static T GetInstance()
            {
                if (_entity is not null)
                {
                    return _entity;
                }

                GameObject gameObject = new GameObject(typeof(T).Name.ToUpper());
                singleton = gameObject.AddComponent<UnitySingleton>();
                _entity = Activator.CreateInstance<T>();
                singleton.Setup(_entity);
                _entity.OnAwake();
                GameObject.DontDestroyOnLoad(gameObject);
                return _entity;
            }
        }

        public void StartCoroutine(IEnumerator enumerator)
        {
            SinglePipeline.singleton?.StartCoroutine(enumerator);
        }

        public void StopAllCoroutine()
        {
            SinglePipeline.singleton?.StopAllCoroutines();
        }

        internal protected virtual void OnAwake()
        {
        }

        internal protected virtual void OnDestroy()
        {
        }

        internal protected virtual void OnUpdate()
        {
        }

        internal protected virtual void OnFixedUpdate()
        {
        }

        internal protected virtual void OnLateUpdate()
        {
        }

        internal protected virtual void OnGUI()
        {
        }

        internal protected virtual void OnApplicationQuit()
        {
        }

        internal protected virtual void OnApplicationPause(bool pause)
        {
        }

        internal protected virtual void OnApplicationFocus(bool focus)
        {
        }
    }
}
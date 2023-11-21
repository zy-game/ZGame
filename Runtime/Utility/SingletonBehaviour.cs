using System;
using System.Collections;
using UnityEngine;

namespace ZGame
{
    public class SingletonBehaviour<T> where T : SingletonBehaviour<T>
    {
        public static T instance => SinglePipeline.GetInstance();

        class SinglePipeline
        {
            public static T _entity;

            public static T GetInstance()
            {
                if (_entity is not null)
                {
                    return _entity;
                }

                _entity = Activator.CreateInstance<T>();
                EventListener.instance.onDestroy.AddListener(_entity.OnDestroy);
                EventListener.instance.update.AddListener(_entity.OnUpdate);
                EventListener.instance.fixedUpdate.AddListener(_entity.OnFixedUpdate);
                EventListener.instance.lateUpdate.AddListener(_entity.OnLateUpdate);
                EventListener.instance.onGUI.AddListener(_entity.OnGUI);
                EventListener.instance.onApplicationQuit.AddListener(_entity.OnApplicationQuit);
                EventListener.instance.onApplicationPause.AddListener(_entity.OnApplicationPause);
                EventListener.instance.onApplicationFocus.AddListener(_entity.OnApplicationFocus);
                return _entity;
            }
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

        public void StartCoroutine(IEnumerator enumerator)
        {
            EventListener.instance?.StartCoroutine(enumerator);
        }

        public void StopAllCoroutine()
        {
            EventListener.instance?.StopAllCoroutines();
        }
    }
}
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
            public static Behaviour _behaviour;

            public static T GetInstance()
            {
                if (_entity is not null)
                {
                    return _entity;
                }

                _entity = Activator.CreateInstance<T>();
                _behaviour = new GameObject(nameof(T)).AddComponent<Behaviour>();
                return _entity;
            }
        }

        class Behaviour : MonoBehaviour
        {
            private void Awake()
            {
                GameObject.DontDestroyOnLoad(this.gameObject);
                SinglePipeline._entity?.OnAwake();
            }

            private void Update()
            {
                SinglePipeline._entity?.OnUpdate();
            }

            private void FixedUpdate()
            {
                SinglePipeline._entity?.OnFixedUpdate();
            }

            private void LateUpdate()
            {
                SinglePipeline._entity?.OnLateUpdate();
            }

            private void OnGUI()
            {
                SinglePipeline._entity?.OnGUI();
            }

            private void OnDestroy()
            {
                SinglePipeline._entity?.OnDestroy();
            }

            private void OnApplicationQuit()
            {
                SinglePipeline._entity?.OnApplicationQuit();
            }

            private void OnApplicationPause(bool pause)
            {
                SinglePipeline._entity?.OnApplicationPause(pause);
            }

            private void OnApplicationFocus(bool focus)
            {
                SinglePipeline._entity?.OnApplicationFocus(focus);
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
            SinglePipeline._behaviour?.StartCoroutine(enumerator);
        }

        public void StopAllCoroutine()
        {
            SinglePipeline._behaviour.StopAllCoroutines();
        }
    }
}
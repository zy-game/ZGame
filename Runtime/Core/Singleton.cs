using System;
using UnityEngine;
using UnityEngine.Events;

namespace ZEngine
{
    public class Singleton<T> : IDisposable where T : Singleton<T>, new()
    {
        public static T instance => SingletonHandle.GetInstance();

        internal class SingletonHandle
        {
            private static T _instance;

            public static T GetInstance()
            {
                if (_instance is not null)
                {
                    return _instance;
                }

                _instance = new T();
                Init();
                return _instance;
            }

            private static void Init()
            {
                if (Application.isPlaying is false)
                {
                    return;
                }

                UnityEngineContent.instance.OnUpdate(_instance.OnUpdate);
                UnityEngineContent.instance.OnApplicationQuiting(Dispose);
                UnityEngineContent.instance.OnLateUpdate(_instance.OnLateUpdate);
                UnityEngineContent.instance.OnFixedUpdate(_instance.OnFixedUpdate);
                UnityEngineContent.instance.OnApplicationFocusChange(_instance.OnFocusChange);
            }

            private static void Dispose()
            {
                if (_instance is null)
                {
                    return;
                }

                _instance.Dispose();
                UnityEngineContent.instance.RemoveUpdate(_instance.OnUpdate);
                UnityEngineContent.instance.RemoveUpdate(_instance.OnLateUpdate);
                UnityEngineContent.instance.RemoveUpdate(_instance.OnFixedUpdate);
                UnityEngineContent.instance.RemoveApplicationQuit(_instance.Dispose);
                UnityEngineContent.instance.RemoveApplicationFocus(_instance.OnFocusChange);
                _instance = null;
            }
        }

        public virtual void Dispose()
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

        protected virtual void OnFocusChange(bool focus)
        {
        }
    }
}
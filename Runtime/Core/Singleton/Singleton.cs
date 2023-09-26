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

                UnityBehaviour.instance.OnUpdate(_instance.OnUpdate);
                UnityBehaviour.instance.OnApplicationQuiting(Dispose);
                UnityBehaviour.instance.OnLateUpdate(_instance.OnLateUpdate);
                UnityBehaviour.instance.OnFixedUpdate(_instance.OnFixedUpdate);
                UnityBehaviour.instance.OnApplicationFocusChange(_instance.OnFocusChange);
            }

            private static void Dispose()
            {
                if (_instance is null)
                {
                    return;
                }

                _instance.Dispose();
                UnityBehaviour.instance.RemoveUpdate(_instance.OnUpdate);
                UnityBehaviour.instance.RemoveUpdate(_instance.OnLateUpdate);
                UnityBehaviour.instance.RemoveUpdate(_instance.OnFixedUpdate);
                UnityBehaviour.instance.RemoveApplicationQuit(_instance.Dispose);
                UnityBehaviour.instance.RemoveApplicationFocus(_instance.OnFocusChange);
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
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
                if (Application.isPlaying is true)
                {
                    UnityBehaviour.instance.OnUpdate(_instance.OnUpdate);
                    UnityBehaviour.instance.OnApplicationQuit(_instance.Dispose);
                    UnityBehaviour.instance.OnLateUpdate(_instance.OnLateUpdate);
                    UnityBehaviour.instance.OnFixedUpdate(_instance.OnFixedUpdate);
                    UnityBehaviour.instance.OnApplicationFocus(_instance.OnFocusChange);
                }

                return _instance;
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
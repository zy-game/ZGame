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

                BehaviourSingleton.OnUpdate(_instance.OnUpdate);
                BehaviourSingleton.OnApplicationQuiting(Dispose);
                BehaviourSingleton.OnLateUpdate(_instance.OnLateUpdate);
                BehaviourSingleton.OnFixedUpdate(_instance.OnFixedUpdate);
                BehaviourSingleton.OnApplicationFocusChange(_instance.OnFocusChange);
            }

            private static void Dispose()
            {
                if (_instance is null)
                {
                    return;
                }

                _instance.Dispose();
                BehaviourSingleton.RemoveUpdate(_instance.OnUpdate);
                BehaviourSingleton.RemoveUpdate(_instance.OnLateUpdate);
                BehaviourSingleton.RemoveUpdate(_instance.OnFixedUpdate);
                BehaviourSingleton.RemoveApplicationQuit(_instance.Dispose);
                BehaviourSingleton.RemoveApplicationFocus(_instance.OnFocusChange);
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
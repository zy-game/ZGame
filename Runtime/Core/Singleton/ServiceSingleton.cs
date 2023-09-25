using System;
using UnityEngine;

namespace ZEngine
{
    public class ServiceSingleton<T> : IDisposable where T : ServiceSingleton<T>, new()
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
                    UnityFunctionLinker.instance.quit.AddListener(_instance.Dispose);
                    UnityFunctionLinker.instance.fixedEvent.AddListener(_instance.OnFixedUpdate);
                    UnityFunctionLinker.instance.updateEvent.AddListener(_instance.OnUpdate);
                    UnityFunctionLinker.instance.lateEvent.AddListener(_instance.OnLateUpdate);
                    UnityFunctionLinker.instance.focusEvent.AddListener(_instance.OnFocus);
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

        protected virtual void OnFocus(bool focus)
        {
        }

        public virtual void SchedulerCommand(ICommand command)
        {
        }
    }
}
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ZGame
{
    public class Singleton<T> : IDisposable where T : Singleton<T>, new()
    {
        public static T instance => GetInstance();
        private static T _instance;

        private static T GetInstance()
        {
            if (_instance is not null)
            {
                return _instance;
            }

            _instance = Activator.CreateInstance<T>();
            BehaviourScriptable.instance.SetupOnDestroy(_instance.Dispose);
            _instance.OnAwake();
            return _instance;
        }

      

        protected virtual void OnAwake()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
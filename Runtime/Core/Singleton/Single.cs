using System;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;


public class Single<T> : IDisposable where T : Single<T>, new()
{
    private class SingletonHandle
    {
        private static T _instance;

        public static T GetInstance()
        {
            if (_instance is null)
            {
                _instance = new T();
                UnityEventArgs.Subscribe(UnityEventArgs.EventType.OnApplicationQuit, args => _instance.Dispose());
            }

            return _instance;
        }
    }


    public static T instance => SingletonHandle.GetInstance();

    public virtual void Dispose()
    {
    }
}
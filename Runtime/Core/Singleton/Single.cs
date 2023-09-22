using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZEngine;

class Linker : MonoBehaviour
{
    public UnityEvent quit = new UnityEvent();
    public UnityEvent fixedEvent = new UnityEvent();
    public UnityEvent updateEvent = new UnityEvent();
    public UnityEvent lateEvent = new UnityEvent();
    public UnityEvent<bool> focusEvent = new UnityEvent<bool>();
    public UnityEvent destroyEvent = new UnityEvent();
    
    private static GameObject _gameObject;
    private static Linker _linker;

    public static Linker instance
    {
        get
        {
            if (_gameObject == null)
            {
                _gameObject = new GameObject("ZEngine");
                GameObject.DontDestroyOnLoad(_gameObject);
                _linker = _gameObject.AddComponent<Linker>();
            }

            return _linker;
        }
    }

    private void LateUpdate()
    {
        lateEvent?.Invoke();
    }

    private void FixedUpdate()
    {
        fixedEvent?.Invoke();
    }

    private void Update()
    {
        updateEvent?.Invoke();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        focusEvent?.Invoke(hasFocus);
    }

    private void OnApplicationQuit()
    {
        quit?.Invoke();
    }
}

public class Single<T> : IDisposable where T : Single<T>, new()
{
    public static T instance => SingletonHandle.GetInstance();

    internal class SingletonHandle
    {
        private static T _instance;


        public static T GetInstance()
        {
            _instance = new T();
            if (Application.isPlaying is true)
            {
                Linker.instance.quit.AddListener(_instance.Dispose);
                Linker.instance.fixedEvent.AddListener(_instance.OnFixedUpdate);
                Linker.instance.updateEvent.AddListener(_instance.OnUpdate);
                Linker.instance.lateEvent.AddListener(_instance.OnLateUpdate);
                Linker.instance.focusEvent.AddListener(_instance.OnFocus);
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
}
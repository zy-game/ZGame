using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ZGame
{
    public class Singleton<T> where T : Singleton<T>, new()
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
            _instance.Initialize();
            return _instance;
        }

        private void Initialize()
        {
            BehaviourScriptable.instance.SetupOnGUI(OnGUI);
            BehaviourScriptable.instance.SetupUpdate(OnUpdate);
            BehaviourScriptable.instance.SetupOnDestroy(Uninitialize);
            BehaviourScriptable.instance.SetupLateUpdate(OnLateUpdate);
            BehaviourScriptable.instance.SetupFixedUpdate(OnFixedUpdate);
            BehaviourScriptable.instance.SetupOnApplicationQuit(OnApplicationQuit);
            BehaviourScriptable.instance.SetupOnApplicationPause(OnApplicationPause);
            BehaviourScriptable.instance.SetupOnApplicationFocus(OnApplicationFocus);
            OnAwake();
        }

        private void Uninitialize()
        {
            BehaviourScriptable.instance.UnsetupOnGUI(OnGUI);
            BehaviourScriptable.instance.UnsetupUpdate(OnUpdate);
            BehaviourScriptable.instance.SetupOnDestroy(Uninitialize);
            BehaviourScriptable.instance.UnsetupFixedUpdate(OnFixedUpdate);
            BehaviourScriptable.instance.UnsetupLateUpdate(OnLateUpdate);
            BehaviourScriptable.instance.SetupOnApplicationQuit(OnApplicationQuit);
            BehaviourScriptable.instance.SetupOnApplicationPause(OnApplicationPause);
            BehaviourScriptable.instance.SetupOnApplicationFocus(OnApplicationFocus);
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return BehaviourScriptable.instance.StartCoroutine(enumerator);
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            BehaviourScriptable.instance.StopCoroutine(coroutine);
        }

        public void StopAllCoroutine()
        {
            BehaviourScriptable.instance.StopAllCoroutines();
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
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public abstract class UIWindow : IDisposable
    {
        private List<IUIComponentBindPipeline> bindPipelines = new List<IUIComponentBindPipeline>();
        private Dictionary<string, GameObject> childList = new Dictionary<string, GameObject>();
        public GameObject gameObject { get; internal set; }

        public virtual void Awake()
        {
        }

        public virtual void Enable()
        {
        }

        public virtual void Disable()
        {
        }

        public virtual void OnEvent(string eventName, params object[] args)
        {
        }

        public GameObject GetChild(string name)
        {
            if (childList.TryGetValue(name, out GameObject gameObject))
            {
                return gameObject;
            }

            Transform transform = this.gameObject.transform.Find(name);
            if (transform != null)
            {
                childList.Add(name, transform.gameObject);
            }

            return transform.gameObject;
        }

        public T GetBindPipeline<T>(string path)
        {
            return default;
        }

        public virtual void Dispose()
        {
            GameObject.DestroyImmediate(gameObject);
            childList.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
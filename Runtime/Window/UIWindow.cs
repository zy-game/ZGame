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
        private Dictionary<string, GameObject> childList = new Dictionary<string, GameObject>();
        private List<IUIBindPipeline> bindPipelines = new List<IUIBindPipeline>();
        public GameObject gameObject { get; private set; }

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

        internal void SetGameObject(GameObject value, IUIWindowOptions windowOptions)
        {
            this.gameObject = value;
            if (windowOptions is null)
            {
                return;
            }

            windowOptions.Initialize(this);
        }

        internal void SetBindPipeline(string path, IUIBindPipeline pipeline)
        {
            IUIBindPipeline bindPipeline = bindPipelines.Find(x => x.path == path);
            if (bindPipeline is not null)
            {
                return;
            }

            bindPipelines.Add(pipeline);
        }

        public virtual void OnNotifyChanged(string path, object args)
        {
            IUIBindPipeline bindPipeline = bindPipelines.Find(x => x.path == path);
            if (bindPipeline is null)
            {
                return;
            }

            bindPipeline.OnChangeValue(args);
        }


        public GameObject GetChild(string name)
        {
            if (childList.TryGetValue(name, out GameObject gameObject))
            {
                return gameObject;
            }

            Transform transform = gameObject.transform.Find(name);
            if (transform != null)
            {
                childList.Add(name, transform.gameObject);
            }

            return transform.gameObject;
        }

        public virtual void Dispose()
        {
            GameObject.DestroyImmediate(gameObject);
            childList.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
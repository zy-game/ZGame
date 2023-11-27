using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    public abstract class UIBase : IDisposable
    {
        public string name { get; }
        public GameObject gameObject { get; }

        public UIBase(GameObject gameObject)
        {
            this.name = gameObject.name;
            this.gameObject = gameObject;
        }

        public virtual void Awake()
        {
        }

        public virtual void Enable()
        {
        }

        public virtual void Disable()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
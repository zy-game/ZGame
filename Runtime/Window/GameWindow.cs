using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    public abstract class GameWindow : IDisposable
    {
        public string name { get; }
        public GameObject gameObject { get; }

        public GameWindow(GameObject gameObject)
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
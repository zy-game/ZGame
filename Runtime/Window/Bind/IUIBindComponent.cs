using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    public class UIBind<T> : IDisposable
    {
        public string name { get; private set; }
        public Transform transform { get; private set; }

        public UIBind(Transform transform)
        {
            if (transform == null)
            {
                return;
            }

            name = transform.name;
            this.transform = transform;
        }

        public void Setup(object args)
        {
        }

        public void SetCallback<T1>(Action<T1> callback)
        {
        }

        public void SetCallback(Action callback)
        {
        }

        public virtual void Dispose()
        {
            this.transform = null;
            name = String.Empty;
        }
    }
}
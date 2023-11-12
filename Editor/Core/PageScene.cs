using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public sealed class DefaultOptions
    {
        public static T GetDefault<T>() where T : ScriptableObject
        {
            return default;
        }
    }


    public class PageScene : IDisposable
    {
        public string name { get; private set; }
        public PageScene parent { get; private set; }
        public Rect position { get; set; }
        public string search { get; set; }

        public PageScene()
        {
            Init();
        }

        void Init()
        {
            BindScene attribute = this.GetType().GetCustomAttribute<BindScene>();
            if (attribute is null)
            {
                return;
            }

            this.name = attribute.name;
            if (attribute.parent is null)
            {
                return;
            }

            this.parent = WindowDocker.GetScene(attribute.parent);
        }

        public virtual void OnEnable(params object[] args)
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnGUI()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
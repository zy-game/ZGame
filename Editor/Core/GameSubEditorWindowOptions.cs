using System;
using UnityEngine;

namespace ZGame.Editor
{
    public interface ICollector<T> : IDisposable
    {
        T[] OnStartCollect(params object[] args);
    }

    public class GameSubEditorWindowOptions : Attribute
    {
        public string name;
        public Type parent;
        public bool isPopup = false;
        public Type configType;
        public string extension;

        public GameSubEditorWindowOptions(string name)
        {
            this.name = name;
        }

        public GameSubEditorWindowOptions(string name, Type parent)
        {
            this.name = name;
            this.parent = parent;
        }

        public GameSubEditorWindowOptions(string name, Type parent, bool isPopup)
        {
            this.name = name;
            this.parent = parent;
            this.isPopup = isPopup;
        }

        public GameSubEditorWindowOptions(string name, Type parent, bool isPopup, Type configType)
        {
            this.name = name;
            this.parent = parent;
            this.isPopup = isPopup;
            this.configType = configType;
        }

        public GameSubEditorWindowOptions(string name, Type parent, bool isPopup, string extension)
        {
            this.name = name;
            this.parent = parent;
            this.isPopup = isPopup;
            this.configType = configType;
            this.extension = extension;
        }
    }
}
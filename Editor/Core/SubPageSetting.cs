using System;
using UnityEngine;

namespace ZGame.Editor
{
    public class SubPageSetting : Attribute
    {
        public string name;
        public Type parent;
        public bool isPopup = false;
        public Type configType;
        public string extension;

        public SubPageSetting(string name)
        {
            this.name = name;
        }

        public SubPageSetting(string name, Type parent)
        {
            this.name = name;
            this.parent = parent;
        }

        public SubPageSetting(string name, Type parent, bool isPopup)
        {
            this.name = name;
            this.parent = parent;
            this.isPopup = isPopup;
        }

        public SubPageSetting(string name, Type parent, bool isPopup, Type configType)
        {
            this.name = name;
            this.parent = parent;
            this.isPopup = isPopup;
            this.configType = configType;
        }
        
        public SubPageSetting(string name, Type parent, bool isPopup, string extension)
        {
            this.name = name;
            this.parent = parent;
            this.isPopup = isPopup;
            this.configType = configType;
            this.extension = extension;
        }
    }
}
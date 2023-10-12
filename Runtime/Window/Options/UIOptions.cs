using System;
using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public sealed class UIOptions : Attribute
    {
        public enum Layer : byte
        {
            Low,
            Middle,
            Top,
        }

        internal Layer layer;
        internal string path;
        internal string localization;

        public UIOptions(string path, Layer layer) : this(path, layer, String.Empty)
        {
        }

        public UIOptions(string path, Layer layer, string localization)
        {
            this.path = path;
            this.layer = layer;
            this.localization = localization;
        }
    }
}
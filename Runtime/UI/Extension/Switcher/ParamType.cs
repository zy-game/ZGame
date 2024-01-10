using System;
using UnityEngine;

namespace ZGame.Window
{
    public enum ParamType
    {
        Int,
        Float,
        String,
        Bool,
        Vector2,
        Vector3,
        Vector4,
        Color,
    }

    [Serializable]
    public class SwitchOptions
    {
        public bool isOn;
        public object userData;
        public ParamType paramType;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
        public string activeText;
        public string inactiveText;
        public GameObject gameObject;
    }

    public enum SwitchType2 : byte
    {
        Sprite,
        Text,
        GameObject,
    }

    public enum SwitchType : byte
    {
        Single,
        Multiple,
    }
}
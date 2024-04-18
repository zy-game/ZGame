using System;
using UnityEngine;

namespace ZGame.UI
{

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

   
}
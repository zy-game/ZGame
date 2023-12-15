using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Game.Baker
{
    [Serializable]
    public class BakerData
    {
        public GameObject gameObject;
        public List<string> components;
#if UNITY_EDITOR
        public List<Type> componentList;
#endif
    }
}
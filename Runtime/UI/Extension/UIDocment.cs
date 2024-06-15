using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ZGame.UI
{
    public class UIDocment : MonoBehaviour
    {
#if UNITY_EDITOR
        public string NameSpace;

        public UnityEngine.Object output;

        public List<GameObject> bindList = new List<GameObject>();


        public string RealName
        {
            get { return name.Split("_").LastOrDefault(); }
        }

        
#endif
    }
}
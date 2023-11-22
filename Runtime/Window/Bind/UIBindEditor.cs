using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Window
{
    [Serializable]
    public class UIBindData
    {
        public string name;
        public string path;
        public List<string> type = new List<string>();
        [NonSerialized] public GameObject target;
    }

    [Serializable]
    public class UIBindConfig
    {
        [SerializeField] public string NameSpace;
        [SerializeField] public UnityEngine.Object output;
        [SerializeField] public List<string> reference = new List<string>();
        [SerializeField] public List<UIBindData> options = new List<UIBindData>();
    }

    public class UIBindEditor : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] public UIBindConfig BindConfig;
#endif
    }
}
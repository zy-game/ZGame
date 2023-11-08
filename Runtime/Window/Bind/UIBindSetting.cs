using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Window
{
    [Serializable]
    public class BindOptions
    {
        public string name;
        public string path;
        public List<string> type = new List<string>();
        public int language;
        [NonSerialized] public GameObject target;
    }

    public class UIBindSetting : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] public List<string> nameSpace = new List<string>();
        [SerializeField] public string NameSpace;
        [SerializeField] public UnityEngine.Object output;
        [SerializeField] public List<BindOptions> options = new List<BindOptions>();
#endif
    }
}
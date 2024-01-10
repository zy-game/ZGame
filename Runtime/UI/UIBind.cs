using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace ZGame.Window
{
    public class UIBind : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] public TextAsset language;
        [SerializeField] public string NameSpace;
        [SerializeField] public bool templetee;
        [SerializeField] public UnityEngine.Object output;
        [SerializeField] public List<string> reference = new List<string>();
        [SerializeField] public List<UIBindData> options = new List<UIBindData>();
#endif
    }

    [Serializable]
    public class UIBindData
    {
        public string name;
        public string path;
        public int language;
        public bool bindLanguage;
        public Selector selector;
        [NonSerialized] public GameObject target;
        [NonSerialized] public bool isOn;
    }
}
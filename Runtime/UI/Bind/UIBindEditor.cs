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
        public int language;
        public Selector selector;
        [NonSerialized] public GameObject target;
    }

    [Serializable]
    public class Selector
    {
        public List<string> items = new List<string>();
        public List<string> reference = new List<string>();

        public bool isAll
        {
            get { return reference.Count == items.Count; }
        }

        public bool isNone
        {
            get { return reference.Count == 0; }
        }


        public override string ToString()
        {
            if (isAll)
            {
                return "Everyting";
            }

            if (isNone)
            {
                return "Nothing";
            }

            return string.Join(",", reference);
        }

        public void Clear()
        {
            reference.Clear();
        }

        public void Add(string name)
        {
            items.Add(name);
        }

        public void Remove(string name)
        {
            items.Remove(name);
            reference.Remove(name);
        }

        public void Select(string name)
        {
            reference.Add(name);
        }

        public void UnSelect(string name)
        {
            reference.Remove(name);
        }

        public void SelectAll()
        {
            reference.AddRange(items);
        }

        public bool IsSelected(string name)
        {
            return reference.Contains(name);
        }
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
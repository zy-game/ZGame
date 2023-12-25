using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Window
{
    public class UIBind : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] public string NameSpace;
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

    [Serializable]
    public class SelectorData
    {
        public string name;
        public bool isOn;
    }

    [Serializable]
    public class Selector
    {
        public List<SelectorData> items = new List<SelectorData>();

        public bool isAll
        {
            get { return items.Exists(x => x.isOn == false) is false; }
        }

        public bool isNone
        {
            get { return items.Where(x => x.isOn).Count() == 0; }
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

            return string.Join(",", items.Select(x => x.name));
        }

        public void Clear()
        {
            items.Clear();
        }


        public void Add(params string[] args)
        {
            foreach (var VARIABLE in args)
            {
                if (items.Exists(x => x.name == VARIABLE))
                {
                    continue;
                }

                items.Add(new SelectorData() { name = VARIABLE, isOn = false });
            }
        }

        public void Remove(string name)
        {
            SelectorData selected = items.Find(x => x.name == name);
            if (selected is null)
            {
                return;
            }

            items.Remove(selected);
        }

        public void Select(string name)
        {
            SelectorData selected = items.Find(x => x.name == name);
            if (selected is null)
            {
                return;
            }

            selected.isOn = true;
        }

        public void UnSelect(string name)
        {
            SelectorData selected = items.Find(x => x.name == name);
            if (selected is null)
            {
                return;
            }

            selected.isOn = false;
        }

        public void SelectAll()
        {
            foreach (var item in items)
            {
                item.isOn = true;
            }
        }

        public void UnSelectAll()
        {
            items.ForEach(x => x.isOn = false);
        }

        public bool IsSelected(string name)
        {
            SelectorData selected = items.Find(x => x.name == name);
            if (selected is null)
            {
                return false;
            }

            return selected.isOn;
        }
    }
}
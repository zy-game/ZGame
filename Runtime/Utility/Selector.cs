using System;
using System.Collections.Generic;
using System.Linq;

namespace ZGame
{
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

        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= items.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                return items[index].name;
            }
        }

        public bool isAll
        {
            get
            {
                if (Count == 0)
                {
                    return false;
                }

                return items.Any(x => x.isOn);
            }
        }

        public bool isNone
        {
            get { return Count == 0 || items.Any(x => x.isOn is false); }
        }

        public string[] Selected
        {
            get { return items.Where(x => x.isOn).Select(x => x.name).ToArray(); }
        }

        public int Count
        {
            get
            {
                if (items is null)
                {
                    return 0;
                }

                return items.Count;
            }
        }


        public Selector(params string[] args)
        {
            Add(args);
        }

        public override string ToString()
        {
            if (isAll && items.Count > 5)
            {
                return "Everyting";
            }

            if (isNone)
            {
                return "Nothing";
            }


            return string.Join(",", items.Where(x => x.isOn).Select(x => x.name));
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(string name)
        {
            return items.Exists(x => x.name == name);
        }

        public void Add(params string[] args)
        {
            if (items is null)
            {
                items = new List<SelectorData>();
            }

            List<string> temp = args.ToList();
            for (int i = 0; i < items.Count; i++)
            {
                if (temp.Contains(items[i].name))
                {
                    continue;
                }

                items.Remove(items[i]);
            }

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
            if (items is null)
            {
                items = new List<SelectorData>();
            }

            SelectorData selected = items.Find(x => x.name == name);
            if (selected is null)
            {
                return;
            }

            items.Remove(selected);
        }

        public void Select(string name)
        {
            if (items is null)
            {
                items = new List<SelectorData>();
            }

            SelectorData selected = items.Find(x => x.name == name);
            if (selected is null)
            {
                return;
            }

            selected.isOn = true;
        }

        public void UnSelect(string name)
        {
            if (items is null)
            {
                items = new List<SelectorData>();
            }

            SelectorData selected = items.Find(x => x.name == name);
            if (selected is null)
            {
                return;
            }

            selected.isOn = false;
        }

        public void SelectAll()
        {
            if (items is null)
            {
                items = new List<SelectorData>();
            }

            foreach (var item in items)
            {
                item.isOn = true;
            }
        }

        public void UnSelectAll()
        {
            if (items is null)
            {
                items = new List<SelectorData>();
            }

            items.ForEach(x => x.isOn = false);
        }

        public bool IsSelected(string name)
        {
            if (items is null)
            {
                items = new List<SelectorData>();
            }

            SelectorData selected = items.Find(x => x.name == name);
            if (selected is null)
            {
                return false;
            }

            return selected.isOn;
        }

#if UNITY_EDITOR
        public void ShowContext(Action action)
        {
            UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
            menu.AddItem(new UnityEngine.GUIContent("Noting"), isNone, () =>
            {
                UnSelectAll();
                action?.Invoke();
            });
            menu.AddItem(new UnityEngine.GUIContent("Everyting"), isAll, () =>
            {
                SelectAll();
                action?.Invoke();
            });
            foreach (var oss in items)
            {
                menu.AddItem(new UnityEngine.GUIContent(oss.name), oss.isOn, () =>
                {
                    if (IsSelected(oss.name))
                    {
                        UnSelect(oss.name);
                    }
                    else
                    {
                        Select(oss.name);
                    }

                    action?.Invoke();
                });
            }

            menu.ShowAsContext();
        }
#endif
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ZGame.Window
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

    public class UISwitcher : MonoBehaviour
    {
        public SwitchType type;
        public UISwitcherTemplate template;
        public List<UISwitcherTemplate> tables = new List<UISwitcherTemplate>();
        public UnityEvent<object> onSelect;


        private void Awake()
        {
            if (tables is null || tables.Count == 0)
            {
                return;
            }

            Initialized();

            if (type == SwitchType.Multiple)
            {
                tables.ForEach(x => x.Unselect());
                return;
            }

            OnSelectDontNotify(0);
        }

        private void Initialized()
        {
            for (int i = 0; i < tables.Count; i++)
            {
                var item = tables[i];
                Button btn = item.GetComponent<Button>();
                int index = i;
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => Select(index));
                }
            }
        }

        public void OnSelectDontNotify(int index)
        {
            if (index < 0 || index > tables.Count - 1)
            {
                return;
            }

            if (tables[index] == null)
            {
                return;
            }

            if (type == SwitchType.Single)
            {
                tables.ForEach(x => x.Unselect());
                tables[index].Select();
                return;
            }

            if (type == SwitchType.Multiple)
            {
                tables[index].Select();
                return;
            }
        }

        public List<object> GetSeleected()
        {
            List<object> list = new List<object>();
            for (int i = 0; i < tables.Count; i++)
            {
                if (tables[i].isSelect)
                {
                    list.Add(tables[i].param);
                }
            }

            return list;
        }

        public void Select(int index)
        {
            if (index < 0 || index > tables.Count - 1)
            {
                return;
            }

            if (tables[index] == null)
            {
                return;
            }

            if (type == SwitchType.Single)
            {
                tables.ForEach(x => x.Unselect());
                tables[index].Select();
                if (onSelect != null)
                {
                    onSelect.Invoke(tables[index].param);
                }

                return;
            }

            if (tables[index].isSelect)
            {
                tables[index].Unselect();
                return;
            }

            tables[index].Select();
        }

        public void Add(params SwitchOptions[] args)
        {
            if (template == null)
            {
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                var item = Instantiate(template, transform);
                item.options = args[i];
                item.type = template.type;
                item.paramType = args[i].paramType;
                item.isSelect = false;
                switch (args[i].paramType)
                {
                    case ParamType.Int:
                        item._v1 = (int)args[i].userData;
                        break;
                    case ParamType.Float:
                        item._v2 = (float)args[i].userData;
                        break;
                    case ParamType.String:
                        item._v3 = (string)args[i].userData;
                        break;
                    case ParamType.Bool:
                        item._v4 = (bool)args[i].userData;
                        break;
                    case ParamType.Vector2:
                        item._v5 = (Vector2)args[i].userData;
                        break;
                    case ParamType.Vector3:
                        item._v6 = (Vector3)args[i].userData;
                        break;
                    case ParamType.Vector4:
                        item._v7 = (Vector4)args[i].userData;
                        break;
                    case ParamType.Color:
                        item._v8 = (Color)args[i].userData;
                        break;
                }

                tables.Add(item);
                item.Unselect();
            }
        }

        public UISwitcherTemplate GetTemplate(int index)
        {
            if (index < 0 || index > tables.Count - 1)
            {
                return null;
            }

            return tables[index];
        }

        public void Clear()
        {
            tables.Clear();
        }
    }
}
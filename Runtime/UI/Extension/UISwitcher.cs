using System;
using System.Collections.Generic;
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
        private List<object> _userData = new List<object>();

        public List<object> selected
        {
            get { return _userData; }
        }

        private void Awake()
        {
            if (tables is null || tables.Count == 0)
            {
                return;
            }

            Initialized();

            if (type == SwitchType.Multiple)
            {
                _userData.Clear();
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
                if (!_userData.Contains(tables[index].options.userData))
                {
                    _userData.Add(tables[index].options.userData);
                }

                return;
            }
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
                    onSelect.Invoke(tables[index].options.userData);
                }

                return;
            }

            if (_userData.Contains(tables[index].options.userData))
            {
                tables[index].Unselect();
                _userData.Remove(tables[index].options.userData);
                return;
            }

            tables[index].Select();
            _userData.Add(tables[index].options.userData);
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
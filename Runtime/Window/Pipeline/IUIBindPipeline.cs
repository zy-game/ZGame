using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public enum UIBindType : byte
    {
        Button,
        Image,
        RawImage,
        Text,
        Slider,
        Toggle,
        Dropdown,
        DropdownTemplate,
        InputField,
        DataList,
        DataItem,
    }

    public interface IUIBindPipeline : IDisposable
    {
        string path { get; }
        string name { get; }
        object value { get; }
        GameObject gameObject { get; }

        void Active();
        void Inactive();
        void OnChange(object args);

        public static IUIBindPipeline Create(UIWindow window, string path, UIBindType type)
        {
            IUIBindPipeline bindPipeline = default;
            switch (type)
            {
                case UIBindType.Button:
                case UIBindType.Image:
                case UIBindType.Slider:
                case UIBindType.Text:
                case UIBindType.Toggle:
                case UIBindType.InputField:
                case UIBindType.RawImage:
                    bindPipeline = new NormalBindPipelineHandle(window, path, type);
                    break;
                case UIBindType.Dropdown:
                    bindPipeline = IUIDropdownBindPipeline.Create(window, path);
                    break;
                case UIBindType.DropdownTemplate:
                    bindPipeline = IUIDropdownTemplate.Create(window, path);
                    break;
                case UIBindType.DataList:
                    bindPipeline = IUIDataListViewBindPipeline.Create(window, path);
                    break;
                case UIBindType.DataItem:
                    bindPipeline = IUIViewDataTemplate.Create(window, path);
                    break;
            }

            return bindPipeline;
        }

        class NormalBindPipelineHandle : IUIBindPipeline
        {
            public string path { get; set; }
            public string name { get; set; }
            public object value { get; set; }
            public GameObject gameObject { get; set; }
            private UIBindType type;
            private UIWindow window;
            private Behaviour component;

            public NormalBindPipelineHandle(UIWindow window, string path, UIBindType type)
            {
                this.type = type;
                this.path = path;
                this.window = window;
                this.name = gameObject.name;
                this.gameObject = window.GetChild(path);
                Active();
            }


            public void Active()
            {
                if (gameObject == null)
                {
                    return;
                }

                switch (type)
                {
                    case UIBindType.Button:
                        Button btn = gameObject.GetComponent<Button>();
                        if (btn == null)
                        {
                            return;
                        }

                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => { window.OnEvent(btn.name); });
                        component = btn;
                        break;
                    case UIBindType.Slider:
                        Slider slider = gameObject.GetComponent<Slider>();
                        if (slider == null)
                        {
                            return;
                        }

                        slider.onValueChanged.RemoveAllListeners();
                        slider.onValueChanged.AddListener(args => window.OnEvent(slider.name, args));
                        component = slider;
                        break;
                    case UIBindType.Toggle:
                        Toggle toggle = gameObject.GetComponent<Toggle>();
                        if (toggle == null)
                        {
                            return;
                        }

                        toggle.onValueChanged.RemoveAllListeners();
                        toggle.onValueChanged.AddListener(args => window.OnEvent(toggle.name, args));
                        component = toggle;
                        break;
                    case UIBindType.InputField:
                        TMP_InputField inputField = gameObject.GetComponent<TMP_InputField>();
                        if (inputField == null)
                        {
                            InputField inputField2 = gameObject.GetComponent<InputField>();
                            if (inputField2 == null)
                            {
                                return;
                            }

                            inputField2.onEndEdit.RemoveAllListeners();
                            inputField2.onEndEdit.AddListener(args => window.OnEvent(inputField2.name, args));
                            component = inputField2;
                            return;
                        }

                        inputField.onEndEdit.RemoveAllListeners();
                        inputField.onEndEdit.AddListener(args => window.OnEvent(inputField.name, args));
                        component = inputField;
                        break;
                }
            }

            public void Inactive()
            {
            }

            public void OnChange(object args)
            {
                if (gameObject == null)
                {
                    return;
                }

                value = args;
                switch (type)
                {
                    case UIBindType.Button:
                        if (args is bool state)
                        {
                            component.enabled = state;
                            break;
                        }

                        if (args is Sprite sprite)
                        {
                            component.GetComponent<Image>().sprite = sprite;
                            break;
                        }

                        break;
                    case UIBindType.Image:
                        if (args is Sprite sprite2)
                        {
                            component.GetComponent<Image>().sprite = sprite2;
                        }

                        break;
                    case UIBindType.Slider:
                        if (args is float progress)
                        {
                            ((Slider)component).value = progress;
                        }

                        break;
                    case UIBindType.Text:
                        if (component is TMP_Text tmpText)
                        {
                            tmpText.text = args.ToString();
                            break;
                        }

                        if (component is Text text)
                        {
                            text.text = args.ToString();
                            break;
                        }

                        break;
                    case UIBindType.Toggle:
                        if (args is bool b)
                        {
                            ((Toggle)component).isOn = b;
                        }

                        break;
                    case UIBindType.InputField:
                        if (component is TMP_InputField tmpInputField)
                        {
                            tmpInputField.SetTextWithoutNotify(args.ToString());
                            break;
                        }

                        if (component is InputField inputField)
                        {
                            inputField.SetTextWithoutNotify(args.ToString());
                            break;
                        }

                        break;
                    case UIBindType.RawImage:
                        if (args is Texture texture)
                        {
                            ((RawImage)component).texture = texture;
                        }

                        break;
                }
            }

            public void Dispose()
            {
                component = null;
                gameObject = null;
                window = null;
            }
        }
    }

    public interface IUIDropdownBindPipeline : IUIBindPipeline
    {
        void AddItem(IUIDropdownTemplate template);
        void RemoveItem(string title);
        void RemoveItem(Sprite icon);

        public static IUIDropdownBindPipeline Create(UIWindow window, string path)
        {
            return default;
        }
    }

    public interface IUIDropdownTemplate : IUIBindPipeline, ICloneable
    {
        Sprite icon { get; }
        string title { get; }

        void SetTitle(string title);
        void SetIcon(Sprite icon);

        public static IUIDropdownTemplate Create(UIWindow window, string path)
        {
            return default;
        }
    }

    public interface IUIDataListViewBindPipeline : IUIBindPipeline, IEnumerable<IUIViewDataTemplate>
    {
        string templatePath { get; }
        IUIViewDataTemplate this[int index] { get; }
        int count { get; }

        void Add(IUIViewDataTemplate template);
        void Remove(IUIViewDataTemplate template);
        void Remove(int index);

        public static IUIDataListViewBindPipeline Create(UIWindow window, string path)
        {
            return default;
        }
    }

    public interface IUIDataPropertiesBindPipeline : IUIBindPipeline
    {
        public static IUIDataPropertiesBindPipeline Create(UIWindow window, string path)
        {
            return default;
        }
    }

    public interface IUIViewDataTemplate : IUIBindPipeline, IDisposable, ICloneable
    {
        IUIDataPropertiesBindPipeline this[int index] { get; }
        IUIDataPropertiesBindPipeline this[string name] { get; }

        public static IUIViewDataTemplate Create(UIWindow window, string path)
        {
            return default;
        }
    }
}
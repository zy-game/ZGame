using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public interface IUIComponentBindPipeline : IBindPipeline
    {
        string path { get; }
        UIWindow window { get; }
        GameObject gameObject { get; }

        void Enable();
        void Disable();

        public static IUIComponentBindPipeline Create(UIWindow window, string path, UIBindType type)
        {
            return type switch
            {
                UIBindType.Button => IUIButtonBindPipeline.Create(window, path),
                UIBindType.Image => IUISpriteBindPipeline.Create(window, path),
                UIBindType.Slider => IUISliderBindPipeline.Create(window, path),
                UIBindType.Text => IUITextBindPipeline.Create(window, path),
                UIBindType.Toggle => IUIToggleBindPipeline.Create(window, path),
                UIBindType.InputField => IUIInputFieldBindPipeline.Create(window, path),
                UIBindType.RawImage => IUITextureBindPipeline.Create(window, path),
                UIBindType.Dropdown => IUIDropdownTemplate.Create(window, path),
                UIBindType.DropdownTemplate => IUIDropdownTemplate.Create(window, path),
                UIBindType.DataListView => IUIDataListViewBindPipeline.Create(window, path),
                UIBindType.DataItemTemplate => IUIViewDataTemplate.Create(window, path),
            };
        }
    }

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
        DataListView,
        DataItemTemplate,
    }
}
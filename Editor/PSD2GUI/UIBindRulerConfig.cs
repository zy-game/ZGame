using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Config;
using ZGame.Resource;
using ZGame.Window;

namespace ZGame.Editor.PSD2GUI
{
    public enum UIBindRulerType
    {
        None,
        Button,
        Toggle,
        Slider,
        InputField,
        Text,
        Image,
        RawImage,
        ScrollRect,
        Dropdown,
        RectTransform,
        TMP_Text,
        TMP_InputField,
        TMP_Dropdown,
    }

    [Serializable]
    public class UIBindRulerItem
    {
        public string fullName;
        public string prefix;
        public bool isDefault;

        public string ComponentName
        {
            get
            {
                if (string.IsNullOrEmpty(fullName))
                {
                    return string.Empty;
                }

                var index = fullName.LastIndexOf('.');
                if (index < 0)
                {
                    return fullName;
                }

                return fullName.Substring(index + 1);
            }
        }

        public void Gneric(UIBindGneric gneric)
        {
        }
    }

    [Serializable]
    public class ReferenceNameSpace
    {
        public string nameSpace;
        public bool isDefault;
    }

    [ResourceReference("Assets/Settings/UIBindRuler.asset")]
    public class UIBindRulerConfig : SingletonScriptableObject<UIBindRulerConfig>
    {
        public List<UIBindRulerItem> rules;
        public List<ReferenceNameSpace> nameSpaces;

        public override void OnAwake()
        {
            if (nameSpaces is null || nameSpaces.Count == 0)
            {
                nameSpaces = new()
                {
                    new() { nameSpace = "UnityEngine", isDefault = true },
                    new() { nameSpace = "UnityEngine.UI", isDefault = true },
                    new() { nameSpace = "UnityEngine.EventSystems", isDefault = true },
                    new() { nameSpace = "TMPro", isDefault = true },
                    new() { nameSpace = "System", isDefault = true },
                    new() { nameSpace = "ZGame", isDefault = true },
                    new() { nameSpace = "ZGame.Window", isDefault = true },
                    new() { nameSpace = "ZGame.Config", isDefault = true },
                    new() { nameSpace = "ZGame.Resource", isDefault = true },
                };
                Debug.Log("initialize namespace");
            }

            if (rules is null || rules.Count == 0)
            {
                rules = new()
                {
                    new() { fullName = typeof(Button).FullName, prefix = "btn_", isDefault = true },
                    new() { fullName = typeof(Toggle).FullName, prefix = "toggle_", isDefault = true },
                    new() { fullName = typeof(Slider).FullName, prefix = "slider_", isDefault = true },
                    new() { fullName = typeof(InputField).FullName, prefix = "input_", isDefault = true },
                    new() { fullName = typeof(Text).FullName, prefix = "text_", isDefault = true },
                    new() { fullName = typeof(Image).FullName, prefix = "img_", isDefault = true },
                    new() { fullName = typeof(ScrollRect).FullName, prefix = "scroll_", isDefault = true },
                    new() { fullName = typeof(Dropdown).FullName, prefix = "dorp_", isDefault = true },
                    new() { fullName = typeof(UnityEngine.RectTransform).FullName, prefix = "rect_", isDefault = true },
                    new() { fullName = typeof(TextMeshProUGUI).FullName, prefix = "text_", isDefault = true },
                    new() { fullName = typeof(TMP_InputField).FullName, prefix = "input_", isDefault = true },
                    new() { fullName = typeof(TMP_Dropdown).FullName, prefix = "dorp_", isDefault = true },
                    new() { fullName = typeof(TMP_Text).FullName, prefix = "text_", isDefault = true },
                    new() { fullName = typeof(RawImage).FullName, prefix = "raw_", isDefault = true },
                };
                Debug.Log("initialize ruler list");
            }

            // rules.Sort((a, b) => a.isDefault ? -1 : 1);
            // nameSpaces.Sort((a, b) => a.isDefault ? -1 : 1);
            OnSave();
        }

        public UIBindRulerItem GetRule(string fullName)
        {
            foreach (var rule in rules)
            {
                if (rule.fullName == fullName)
                {
                    return rule;
                }
            }

            return null;
        }

        public void AddNameSpace(string nameSpace)
        {
            var ns = new ReferenceNameSpace() { nameSpace = nameSpace, isDefault = false };
            nameSpaces.Add(ns);
        }

        public void AddRule(string fullName, string prefix)
        {
            var rule = GetRule(fullName);
            if (rule is null)
            {
                rule = new UIBindRulerItem();
                rule.fullName = fullName;
                rule.prefix = prefix;
                rules.Add(rule);
            }
        }

        public void RemoveRule(string fullName)
        {
            var rule = GetRule(fullName);
            if (rule != null)
            {
                rules.Remove(rule);
            }
        }

        public void Clear()
        {
            rules.Clear();
        }


        public void GenericUIBindCode(UIBind setting, bool isGenericUICode)
        {
            OnAwake();
            EditorUtility.SetDirty(setting);
            UIBindGneric gneric = new UIBindGneric(setting);
            File.WriteAllText($"{AssetDatabase.GetAssetPath(setting.output)}/UIBind_{setting.name}.cs", gneric.GetBindCode());
            string output = $"{AssetDatabase.GetAssetPath(setting.output)}/UICode_{setting.name}.cs";
            if (isGenericUICode)
            {
                if (File.Exists(output))
                {
                    if (EditorUtility.DisplayDialog("Warning", "当前UI代码文件已经存在, 是否覆盖写入?", "Yes", "No"))
                    {
                        File.WriteAllText(output, gneric.GetOverloadCode());
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(setting.name + " Generid UICode Finishing");
        }
    }
}
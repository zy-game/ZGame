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
        public UIBindRulerType type;
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
                    new() { fullName = typeof(Button).FullName, prefix = "btn_", type = UIBindRulerType.Button, isDefault = true },
                    new() { fullName = typeof(Toggle).FullName, prefix = "toggle_", type = UIBindRulerType.Toggle, isDefault = true },
                    new() { fullName = typeof(Slider).FullName, prefix = "slider_", type = UIBindRulerType.Slider, isDefault = true },
                    new() { fullName = typeof(InputField).FullName, prefix = "input_", type = UIBindRulerType.InputField, isDefault = true },
                    new() { fullName = typeof(Text).FullName, prefix = "text_", type = UIBindRulerType.Text, isDefault = true },
                    new() { fullName = typeof(Image).FullName, prefix = "img_", type = UIBindRulerType.Image, isDefault = true },
                    new() { fullName = typeof(ScrollRect).FullName, prefix = "scroll_", type = UIBindRulerType.ScrollRect, isDefault = true },
                    new() { fullName = typeof(Dropdown).FullName, prefix = "dorp_", type = UIBindRulerType.Dropdown, isDefault = true },
                    new() { fullName = typeof(UnityEngine.RectTransform).FullName, prefix = "rect_", type = UIBindRulerType.RectTransform, isDefault = true },
                    new() { fullName = typeof(TextMeshProUGUI).FullName, prefix = "text_", type = UIBindRulerType.TMP_Text, isDefault = true },
                    new() { fullName = typeof(TMP_InputField).FullName, prefix = "input_", type = UIBindRulerType.TMP_InputField, isDefault = true },
                    new() { fullName = typeof(TMP_Dropdown).FullName, prefix = "dorp_", type = UIBindRulerType.TMP_Dropdown, isDefault = true },
                    new() { fullName = typeof(TMP_Text).FullName, prefix = "text_", type = UIBindRulerType.TMP_Text, isDefault = true },
                };
                Debug.Log("initialize ruler list");
            }

            rules.Sort((a, b) => a.isDefault ? -1 : 1);
            nameSpaces.Sort((a, b) => a.isDefault ? -1 : 1);
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
            List<string> fieldList = new List<string>()
            {
                "\t\tpublic string name { get; }",
                "\t\tpublic GameObject gameObject { get; }",
            };
            List<string> initFieldList = new List<string>();
            List<string> regEventList = new List<string>();
            List<string> callbackList = new List<string>();
            List<string> setupList = new List<string>();
            List<string> disposeList = new List<string>();
            List<string> language = new List<string>();
            foreach (var VARIABLE in setting.options)
            {
                fieldList.Add($"\t\tpublic GameObject {VARIABLE.name};");
                disposeList.Add($"\t\t\t{VARIABLE.name} = null;");
                initFieldList.Add($"\t\t\t{VARIABLE.name} = this.gameObject.transform.Find(\"{VARIABLE.path}\").gameObject;");
                foreach (var VARIABLE2 in VARIABLE.selector.items)
                {
                    if (VARIABLE2.isOn is false)
                    {
                        continue;
                    }

                    var rule = GetRule(VARIABLE2.name);
                    if (rule is null)
                    {
                        continue;
                    }

                    string fieldName = $"{rule.prefix}{VARIABLE.name}";
                    disposeList.Add($"\t\t\t{fieldName} = null;");
                    fieldList.Add($"\t\tpublic {rule.ComponentName} {fieldName};");
                    switch (rule.type)
                    {
                        case UIBindRulerType.Button:
                            initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<Button>();");

                            regEventList.Add($"\t\t\t{fieldName}?.onClick.RemoveAllListeners();");
                            regEventList.Add($"\t\t\t{fieldName}?.onClick.AddListener(on_handle_{VARIABLE.name});");

                            callbackList.Add($"\t\tprotected virtual void on_handle_{VARIABLE.name}()");
                            callbackList.Add("\t\t{");
                            setupList.Add("");
                            callbackList.Add("\t\t}");
                            callbackList.Add("");
                            break;
                        case UIBindRulerType.Toggle:
                            initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<Toggle>();");

                            regEventList.Add($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
                            regEventList.Add($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name});");

                            callbackList.Add($"\t\tprotected virtual void on_handle_{VARIABLE.name}(bool isOn)");
                            callbackList.Add("\t\t{");
                            setupList.Add("");
                            callbackList.Add("\t\t}");
                            callbackList.Add("");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(bool isOn)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.SetIsOnWithoutNotify(isOn);");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            break;
                        case UIBindRulerType.Slider:
                            initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<Slider>();");

                            regEventList.Add($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
                            regEventList.Add($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name});");

                            callbackList.Add($"\t\tprotected virtual void on_handle_{VARIABLE.name}(float value)");
                            callbackList.Add("\t\t{");
                            setupList.Add("");
                            callbackList.Add("\t\t}");
                            callbackList.Add("");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(float value)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.SetValueWithoutNotify(value);");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            break;
                        case UIBindRulerType.TMP_InputField:
                        case UIBindRulerType.InputField:
                            if (rule.type == UIBindRulerType.InputField)
                            {
                                initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<InputField>();");
                            }
                            else
                            {
                                initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<TMP_InputField>();");
                            }

                            regEventList.Add($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
                            regEventList.Add($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name});");

                            callbackList.Add($"\t\tprotected virtual void on_handle_{VARIABLE.name}(string value)");
                            callbackList.Add("\t\t{");
                            setupList.Add("");
                            callbackList.Add("\t\t}");
                            callbackList.Add("");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(string value)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.SetTextWithoutNotify(value);");
                            setupList.Add("\t\t}");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(TMP_FontAsset fontAsset)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.SetGlobalFontAsset(fontAsset);");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(int size)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.SetGlobalPointSize(size);");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            break;
                        case UIBindRulerType.TMP_Dropdown:
                        case UIBindRulerType.Dropdown:
                            if (rule.type == UIBindRulerType.Dropdown)
                            {
                                initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<Dropdown>();");
                            }
                            else
                            {
                                initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<TMP_Dropdown>();");
                            }

                            regEventList.Add($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
                            regEventList.Add($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name});");

                            callbackList.Add($"\t\tprotected virtual void on_handle_{VARIABLE.name}(int value)");
                            callbackList.Add("\t\t{");
                            setupList.Add("");
                            callbackList.Add("\t\t}");
                            callbackList.Add("");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(int index)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.SetValueWithoutNotify(index);");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            setupList.Add($"\t\tpublic void on_add_{VARIABLE.name}(List<string> items)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.AddOptions(items);");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            setupList.Add($"\t\tpublic void on_add_{VARIABLE.name}(List<Sprite> items)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.AddOptions(items);");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            setupList.Add($"\t\tpublic void on_add_{VARIABLE.name}(List<OptionData> items)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.AddOptions(items);");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            break;
                        case UIBindRulerType.Image:
                            initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<Image>();");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(string path)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\tResHandle handle = ResourceManager.instance.LoadAsset(ILocalliztion.Get(00));");
                            setupList.Add($"\t\t\tif (handle.EnsureLoadSuccess() == false)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add($"\t\t\t{fieldName}.sprite = handle.Get<Sprite>({fieldName}.gameObject);");
                            setupList.Add("\t\t}");
                            setupList.Add("");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(Sprite sprite)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.sprite = sprite;");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(Texture2D texture)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);");
                            setupList.Add("\t\t}");
                            setupList.Add("");

                            if (VARIABLE.bindLanguage && VARIABLE.language != 0)
                            {
                                language.Add($"\t\t\t on_setup_{VARIABLE.name}(ILocalliztion.Get({VARIABLE.language}));");
                            }

                            break;
                        case UIBindRulerType.RawImage:
                            initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<RawImage>();");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(string path)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\tResHandle handle = ResourceManager.instance.LoadAsset(ILocalliztion.Get(00));");
                            setupList.Add($"\t\t\tif (handle.EnsureLoadSuccess() == false)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add($"\t\t\t{fieldName}.texture = handle.Get<Texture2D>({fieldName}.gameObject);");
                            setupList.Add("\t\t}");
                            setupList.Add("");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(Sprite sprite)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.texture = sprite.texture;");
                            setupList.Add("\t\t}");
                            setupList.Add("");
                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(Texture2D texture)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.texture = texture;");
                            setupList.Add("\t\t}");
                            setupList.Add("");

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(RendererTexture texture)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.texture = texture;");
                            setupList.Add("\t\t}");
                            setupList.Add("");

                            if (VARIABLE.bindLanguage && VARIABLE.language != 0)
                            {
                                language.Add($"\t\t\t on_setup_{VARIABLE.name}(ILocalliztion.Get({VARIABLE.language}));");
                            }

                            break;
                        case UIBindRulerType.TMP_Text:
                        case UIBindRulerType.Text:
                            if (rule.type == UIBindRulerType.Text)
                            {
                                initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<Text>();");
                            }
                            else
                            {
                                initFieldList.Add($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<TextMeshProUGUI>();");
                            }

                            setupList.Add($"\t\tpublic void on_setup_{VARIABLE.name}(string text)");
                            setupList.Add("\t\t{");
                            setupList.Add($"\t\t\tif ({fieldName}== null)");
                            setupList.Add($"\t\t\t\treturn;");
                            setupList.Add("");
                            setupList.Add($"\t\t\t{fieldName}.text = text;");
                            setupList.Add("\t\t}");
                            setupList.Add("");

                            if (VARIABLE.bindLanguage && VARIABLE.language != 0)
                            {
                                language.Add($"\t\t\t on_setup_{VARIABLE.name}(ILocalliztion.Get({VARIABLE.language}));");
                            }

                            break;
                    }
                }
            }


            StringBuilder sb = new StringBuilder();

            nameSpaces.ForEach(x => sb.AppendLine("using " + x.nameSpace + ";"));
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Createing with " + DateTime.Now.ToString("g"));
            sb.AppendLine("/// by " + SystemInfo.deviceName);
            sb.AppendLine("/// </summary>");
            sb.AppendLine("namespace " + (setting.NameSpace.IsNullOrEmpty() ? "UICode" : setting.NameSpace));
            sb.AppendLine("{");
            sb.AppendLine("\tpublic class UIBind_" + setting.name + " : UIBase");
            sb.AppendLine("\t{");
            fieldList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("");
            sb.AppendLine($"\t\tpublic UIBind_{setting.name}(GameObject gameObject)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tthis.name = gameObject.name;");
            sb.AppendLine("\t\t\tthis.gameObject = gameObject;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tpublic virtual void Awake()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif(this.gameObject == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\t\tOnBindField();");
            sb.AppendLine("\t\t\tOnBindEvents();");
            sb.AppendLine("\t\t\tSetLanguage();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");

            sb.AppendLine("\t\tprivate void SetLanguage()");
            sb.AppendLine("\t\t{");
            language.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t}");

            sb.AppendLine("\t\tpublic virtual void Enable()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tpublic virtual void Disable()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprivate void OnBindField()");
            sb.AppendLine("\t\t{");
            initFieldList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprivate void OnBindEvents()");
            sb.AppendLine("\t\t{");
            regEventList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            callbackList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("");
            setupList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\tpublic virtual void Dispose()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tGC.SuppressFinalize(this);");
            disposeList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            File.WriteAllText($"{AssetDatabase.GetAssetPath(setting.output)}/UIBind_{setting.name}.cs", sb.ToString());

            if (isGenericUICode)
            {
                sb.Clear();
                nameSpaces.ForEach(x => sb.AppendLine("using " + x.nameSpace + ";"));
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// Createing with " + DateTime.Now.ToString("g"));
                sb.AppendLine("/// by " + SystemInfo.deviceName);
                sb.AppendLine("/// </summary>");
                sb.AppendLine("namespace " + (setting.NameSpace.IsNullOrEmpty() ? "UICode" : setting.NameSpace));
                sb.AppendLine("{");
                string temp = AssetDatabase.GetAssetPath(setting);
                if (temp.IsNullOrEmpty() is false)
                {
                    if (temp.Contains("Resources"))
                    {
                        temp = temp.Substring(0, temp.LastIndexOf("."));
                        temp = temp.Substring(temp.IndexOf("Resources"));
                    }

                    sb.AppendLine($"\t[ResourceReference(\"{temp}\")]");
                }

                sb.AppendLine("\tpublic class UICode_" + setting.name + " : UIBind_" + setting.name);
                sb.AppendLine("\t{");
                sb.AppendLine($"\t\tpublic UICode_{setting.name}(GameObject gameObject) : base(gameObject)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t}");
                callbackList.ForEach(x => sb.AppendLine(x.Replace("virtual", "override")));
                sb.AppendLine("\t}");
                sb.AppendLine("}");
                File.WriteAllText($"{AssetDatabase.GetAssetPath(setting.output)}/UICode_{setting.name}.cs", sb.ToString());
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(setting.name + " Generid UICode Finishing");
        }
    }
}
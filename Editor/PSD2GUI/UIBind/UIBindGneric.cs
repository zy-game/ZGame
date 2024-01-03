using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine.Device;
using ZGame.Window;

namespace ZGame.Editor.PSD2GUI
{
    public class UIBindGneric
    {
        private List<string> fieldList;
        private List<string> eventList;
        private List<string> callbackList;
        private List<string> setupList;
        private List<string> disposeList;
        private List<string> language;
        private List<string> initList;
        private UIBind setting;

        public UIBindGneric(UIBind setting)
        {
            this.setting = setting;
            fieldList = new List<string>();
            eventList = new List<string>();
            callbackList = new List<string>();
            setupList = new List<string>();
            disposeList = new List<string>();
            language = new List<string>();
            initList = new List<string>();
            AddField("\t\tpublic string name { get; }");
            AddField("\t\tpublic GameObject gameObject { get; }");
            Init();
        }

        private void AddInit(string str)
        {
            initList.Add(str);
        }

        public void AddField(string field)
        {
            fieldList.Add(field);
        }

        public void AddEvent(string str)
        {
            eventList.Add(str);
        }

        public void AddCallback(string str)
        {
            callbackList.Add(str);
        }

        public void AddSetup(string str)
        {
            setupList.Add(str);
        }

        public void AddDispose(string str)
        {
            disposeList.Add(str);
        }

        public void AddLanguage(string str)
        {
            language.Add(str);
        }

        private void Init()
        {
            foreach (var VARIABLE in setting.options)
            {
                AddField($"\t\tpublic GameObject {VARIABLE.name};");
                AddDispose($"\t\t\t{VARIABLE.name} = null;");
                AddInit($"\t\t\t{VARIABLE.name} = this.gameObject.transform.Find(\"{VARIABLE.path}\").gameObject;");
                foreach (var VARIABLE2 in VARIABLE.selector.items)
                {
                    if (VARIABLE2.isOn is false)
                    {
                        continue;
                    }

                    var rule = UIBindRulerConfig.instance.GetRule(VARIABLE2.name);
                    if (rule is null)
                    {
                        continue;
                    }

                    string fieldName = $"{rule.prefix}{VARIABLE.name}";
                    string typeName = rule.fullName.Substring(rule.fullName.LastIndexOf(".") + 1);

                    AddDispose($"\t\t\t{fieldName} = null;");
                    AddField($"\t\tpublic {rule.ComponentName} {fieldName};");
                    AddInit($"\t\t\t{fieldName} = this.{VARIABLE.name}.GetComponent<{typeName}>();");
                    switch (typeName)
                    {
                        case "Button":
                            GenericButtonComponent(VARIABLE, fieldName);
                            break;
                        case "Toggle":
                            GenericToggleComponent(VARIABLE, fieldName);
                            break;
                        case "Slider":
                            GenericSliderComponent(VARIABLE, fieldName);
                            break;
                        case "TMP_InputField":
                        case "InputField":
                            GenericInputFieldComponent(VARIABLE, fieldName);
                            break;
                        case "TMP_Dropdown":
                        case "Dropdown":
                            GenericDropdownComponent(VARIABLE, fieldName);
                            break;
                        case "Image":
                            GenericImageComponent(VARIABLE, fieldName);
                            break;
                        case "RawImage":
                            GenericRawImageComponent(VARIABLE, fieldName);
                            break;
                        case "TextMeshProUGUI":
                        case "TMP_Text":
                        case "Text":
                            GenericTextComponent(VARIABLE, fieldName);
                            break;
                        case "LongPresseButton":
                            GenericLongPressButtonComponent(VARIABLE, fieldName);
                            break;
                        case "UISwitcher":
                            GenericUISwitcherComponent(VARIABLE, fieldName);
                            break;
                    }
                }
            }
        }

        private void GenericUISwitcherComponent(UIBindData variable, string fieldName)
        {
            AddEvent($"\t\t\t{fieldName}?.onSelect.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onSelect.AddListener(on_handle_switch_{variable.name});");
            AddCallback($"\t\tprotected virtual void on_handle_switch_{variable.name}(object obj)");
            AddCallback("\t\t{");
            AddCallback("");
            AddCallback("\t\t}");
            AddCallback("");
        }

        private void GenericLongPressButtonComponent(UIBindData variable, string fieldName)
        {
            AddEvent($"\t\t\t{fieldName}?.onCancel.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onCancel.AddListener(on_handle_{variable.name}_Cancel);");
            AddCallback($"\t\tprotected virtual void on_handle_{variable.name}_Cancel()");
            AddCallback("\t\t{");
            AddCallback("");
            AddCallback("\t\t}");
            AddCallback("");

            AddEvent($"\t\t\t{fieldName}?.onDown.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onDown.AddListener(on_handle_{variable.name}_Down);");
            AddCallback($"\t\tprotected virtual void on_handle_{variable.name}_Down()");
            AddCallback("\t\t{");
            AddCallback("");
            AddCallback("\t\t}");
            AddCallback("");

            AddEvent($"\t\t\t{fieldName}?.onDown.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onDown.AddListener(on_handle_{variable.name}_Up);");
            AddCallback($"\t\tprotected virtual void on_handle_{variable.name}_Up()");
            AddCallback("\t\t{");
            AddCallback("");
            AddCallback("\t\t}");
            AddCallback("");
        }

        private void GenericButtonComponent(UIBindData VARIABLE, string fieldName)
        {
            AddEvent($"\t\t\t{fieldName}?.onClick.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onClick.AddListener(on_handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_handle_{VARIABLE.name}()");
            AddCallback("\t\t{");
            AddCallback("");
            AddCallback("\t\t}");
            AddCallback("");
        }

        private void GenericToggleComponent(UIBindData VARIABLE, string fieldName)
        {
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_handle_{VARIABLE.name}(bool isOn)");
            AddCallback("\t\t{");
            AddCallback("");
            AddCallback("\t\t}");
            AddCallback("");
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(bool isOn)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetIsOnWithoutNotify(isOn);");
            AddSetup("\t\t}");
            AddSetup("");
        }

        private void GenericSliderComponent(UIBindData VARIABLE, string fieldName)
        {
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_handle_{VARIABLE.name}(float value)");
            AddCallback("\t\t{");
            AddCallback("");
            AddCallback("\t\t}");
            AddCallback("");
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(float value)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetValueWithoutNotify(value);");
            AddSetup("\t\t}");
            AddSetup("");
        }

        private void GenericInputFieldComponent(UIBindData VARIABLE, string fieldName)
        {
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_handle_{VARIABLE.name}(string value)");
            AddCallback("\t\t{");
            AddCallback("");
            AddCallback("\t\t}");
            AddCallback("");
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(string value)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetTextWithoutNotify(value);");
            AddSetup("\t\t}");
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(TMP_FontAsset fontAsset)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetGlobalFontAsset(fontAsset);");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(int size)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetGlobalPointSize(size);");
            AddSetup("\t\t}");
            AddSetup("");
        }

        private void GenericDropdownComponent(UIBindData VARIABLE, string fieldName)
        {
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_handle_{VARIABLE.name}(int value)");
            AddCallback("\t\t{");
            AddCallback("");
            AddCallback("\t\t}");
            AddCallback("");
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(int index)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetValueWithoutNotify(index);");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_add_{VARIABLE.name}(List<string> items)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.AddOptions(items);");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_add_{VARIABLE.name}(List<Sprite> items)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.AddOptions(items);");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_add_{VARIABLE.name}(List<OptionData> items)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.AddOptions(items);");
            AddSetup("\t\t}");
            AddSetup("");
        }

        private void GenericImageComponent(UIBindData VARIABLE, string fieldName)
        {
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(string path)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\tResHandle handle = ResourceManager.instance.LoadAsset(path);");
            AddSetup($"\t\t\tif (handle.IsSuccess() == false)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup($"\t\t\t{fieldName}.sprite = handle.Get<Sprite>({fieldName}.gameObject);");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(Sprite sprite)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.sprite = sprite;");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(Texture2D texture)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);");
            AddSetup("\t\t}");
            AddSetup("");
            if (VARIABLE.bindLanguage && VARIABLE.language != 0)
            {
                AddLanguage($"\t\t\t on_setup_{VARIABLE.name}(Localliztion.Get({VARIABLE.language}));");
            }
        }

        private void GenericRawImageComponent(UIBindData VARIABLE, string fieldName)
        {
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(string path)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\tResHandle handle = ResourceManager.instance.LoadAsset(path);");
            AddSetup($"\t\t\tif (handle.IsSuccess() == false)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup($"\t\t\t{fieldName}.texture = handle.Get<Texture2D>({fieldName}.gameObject);");
            AddSetup("\t\t}");
            AddSetup("");

            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(Sprite sprite)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.texture = sprite.texture;");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(Texture2D texture)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.texture = texture;");
            AddSetup("\t\t}");
            AddSetup("");

            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(RenderTexture texture)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.texture = texture;");
            AddSetup("\t\t}");
            AddSetup("");

            if (VARIABLE.bindLanguage && VARIABLE.language != 0)
            {
                AddLanguage($"\t\t\t on_setup_{VARIABLE.name}(Localliztion.Get({VARIABLE.language}));");
            }
        }

        private void GenericTextComponent(UIBindData VARIABLE, string fieldName)
        {
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(object info)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.text = info == null ? \"\" : info.ToString();");
            AddSetup("\t\t}");
            AddSetup("");

            if (VARIABLE.bindLanguage && VARIABLE.language != 0)
            {
                AddLanguage($"\t\t\t on_setup_{VARIABLE.name}(Localliztion.Get({VARIABLE.language}));");
            }
        }


        public string GetOverloadCode()
        {
            StringBuilder sb = new StringBuilder();
            UIBindRulerConfig.instance.nameSpaces.ForEach(x => sb.AppendLine("using " + x.nameSpace + ";"));
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
            return sb.ToString();
        }


        public string GetBindCode()
        {
            StringBuilder sb = new StringBuilder();

            UIBindRulerConfig.instance.nameSpaces.ForEach(x => sb.AppendLine("using " + x.nameSpace + ";"));
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
            sb.AppendLine("\t\tpublic virtual void Awake(params object[] args)");
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
            initList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprivate void OnBindEvents()");
            sb.AppendLine("\t\t{");
            eventList.ForEach(x => sb.AppendLine(x));
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
            return sb.ToString();
        }
    }
}
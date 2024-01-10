using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using ZGame.Window;
using SystemInfo = UnityEngine.Device.SystemInfo;

namespace ZGame.Editor.PSD2GUI
{
    public class UIBindGeneric
    {
        private List<string> fieldList;
        private List<string> eventList;
        private List<string> callbackList;
        private List<string> setupList;
        private List<string> disposeList;
        private List<string> language;
        private List<string> initList;
        private List<string> templeteList;
        private UIBind setting;

        public UIBindGeneric(UIBind setting)
        {
            this.setting = setting;
            Clear();
            GenericAllCode();
        }

        public void Clear()
        {
            templeteList = new List<string>();
            fieldList = new List<string>();
            eventList = new List<string>();
            callbackList = new List<string>();
            setupList = new List<string>();
            disposeList = new List<string>();
            language = new List<string>();
            initList = new List<string>();
        }

        private void GenericAllCode()
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

                    if (UIBindRulerConfig.instance.TryGetRuler(VARIABLE2.name, out var rule) is false)
                    {
                        continue;
                    }

                    GenericInitializedCode(VARIABLE, rule);

                    switch (rule.TypeName)
                    {
                        case "Button":
                            GenericButtonComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "Toggle":
                            GenericToggleComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "Slider":
                            GenericSliderComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "TMP_InputField":
                        case "InputField":
                            GenericInputFieldComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "TMP_Dropdown":
                        case "Dropdown":
                            GenericDropdownComponent(VARIABLE, rule.GetFieldName(VARIABLE.name), rule.TypeName);
                            break;
                        case "Image":
                            GenericImageComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "RawImage":
                            GenericRawImageComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "TextMeshProUGUI":
                        case "TMP_Text":
                        case "Text":
                            GenericTextComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "LongPresseButton":
                            GenericLongPressButtonComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "UISwitcher":
                            GenericUISwitcherComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "UIToolbar":
                            break;
                        case "UIPopMsg":
                            GenericPopMsgComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case "UIBind":
                            GenericTempleteComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                    }
                }
            }
        }

        private void GenericInitializedCode(UIBindData variable, UIBindRulerItem rule)
        {
            if (rule.TypeName.Equals("UIBind"))
            {
                Transform target = setting.transform.Find(variable.path);
                if (target == null)
                {
                    return;
                }

                UIBind bindData = target.GetComponent<UIBind>();
                if (bindData == null)
                {
                    return;
                }

                AddField($"\t\tpublic Templete_{bindData.NameSpace} temp_{bindData.NameSpace};");
                AddInit($"\t\t\ttemp_{bindData.NameSpace} = new Templete_{bindData.NameSpace}(this.gameObject.transform.Find(\"{variable.path}\").gameObject);");
                AddDispose($"\t\t\ttemp_{bindData.NameSpace} = null;");
            }
            else
            {
                AddField($"\t\tpublic {rule.TypeName} {rule.GetFieldName(variable.name)};");
                AddInit($"\t\t\t{rule.GetFieldName(variable.name)} = this.{variable.name}.GetComponent<{rule.TypeName}>();");
                AddDispose($"\t\t\t{rule.GetFieldName(variable.name)} = null;");
            }
        }

        private void GenericToolbarComponent(UIBindData variable, string fieldName)
        {
            AddSetup($"\t\tpublic void on_{fieldName}_switch(string sceneName)");
            AddSetup($"\t\t{{");
            AddSetup($"\t\t\tif (this.{fieldName} == null)");
            AddSetup($"\t\t\t{{");
            AddSetup($"\t\t\t\treturn;");
            AddSetup($"\t\t\t}}");
            AddSetup($"\t\t\tthis.{fieldName}.Switch(sceneName);");
            AddSetup($"\t\t}}");
            AddSetup("");
        }

        private void GenericTempleteComponent(UIBindData variable, string fieldName)
        {
            Transform target = setting.transform.Find(variable.path);
            if (target == null)
            {
                return;
            }

            UIBind bindData = target.GetComponent<UIBind>();
            if (bindData == null)
            {
                return;
            }

            UIBindGeneric generic = new UIBindGeneric(bindData);
            templeteList.Add(generic.GetTempleteCode());
        }

        private void GenericPopMsgComponent(UIBindData variable, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent<object> _event_{fieldName}_completion;");
            AddInit($"\t\t\t_event_{fieldName}_completion = new UnityEvent<object>();");
            AddField($"\t\tprivate UnityEvent<object> _event_{fieldName}_play;");
            AddInit($"\t\t\t_event_{fieldName}_play = new UnityEvent<object>();");
            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Complete(UnityAction<object> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_completion.AddListener(callback);");
            AddSetup("\t\t}");

            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Play(UnityAction<object> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_completion.AddListener(callback);");
            AddSetup("\t\t}");

            AddEvent($"\t\t\t{fieldName}?.onCompletion.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onCompletion.AddListener(on_Handle_popmsg_{variable.name}_Complete);");
            AddEvent($"\t\t\t{fieldName}?.onShowPopMsg.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onShowPopMsg.AddListener(on_Handle_popmsg_{variable.name}_Play);");
            AddCallback($"\t\tprotected virtual void on_Handle_PopMsg_{variable.name}_Play(object obj)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_play.Invoke(obj);");
            AddCallback("\t\t}");
            AddCallback("");
            AddCallback($"\t\tprotected virtual void on_Handle_PopMsg_{variable.name}_Complete(object obj)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_completion.Invoke(obj);");
            AddCallback("\t\t}");
            AddCallback("");
        }

        private void GenericUISwitcherComponent(UIBindData variable, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent<object> _event_{fieldName}_switch;");
            AddInit($"\t\t\t_event_{fieldName}_switch = new UnityEvent<object>();");

            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Switch(UnityAction<object> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_switch.AddListener(callback);");
            AddSetup("\t\t}");


            AddEvent($"\t\t\t{fieldName}?.onSelect.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onSelect.AddListener(on_Handle_Switch_{variable.name});");
            AddCallback($"\t\tprotected virtual void on_Handle_Switch_{variable.name}(object obj)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_switch.Invoke(obj);");
            AddCallback("\t\t}");
            AddCallback("");
        }

        private void GenericLongPressButtonComponent(UIBindData variable, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent _event_{fieldName}_Cancel;");
            AddInit($"\t\t\t_event_{fieldName}_Cancel = new UnityEvent();");
            AddField($"\t\tprivate UnityEvent _event_{fieldName}_Down;");
            AddInit($"\t\t\t_event_{fieldName}_Down = new UnityEvent();");
            AddField($"\t\tprivate UnityEvent _event_{fieldName}_Up;");
            AddInit($"\t\t\t_event_{fieldName}_Up = new UnityEvent();");
            AddField($"\t\tprivate UnityEvent _event_{fieldName}_Click;");
            AddInit($"\t\t\t_event_{fieldName}_Click = new UnityEvent();");

            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Cancel(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_Cancel.AddListener(callback);");
            AddSetup("\t\t}");

            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Down(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_Down.AddListener(callback);");
            AddSetup("\t\t}");

            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Up(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_Up.AddListener(callback);");
            AddSetup("\t\t}");

            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Click(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_Click.AddListener(callback);");
            AddSetup("\t\t}");


            AddEvent($"\t\t\t{fieldName}?.onCancel.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onCancel.AddListener(on_Handle_{variable.name}_Cancel);");
            AddCallback($"\t\tprotected virtual void on_Handle_{variable.name}_Cancel()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_Cancel.Invoke();");
            AddCallback("\t\t}");
            AddCallback("");

            AddEvent($"\t\t\t{fieldName}?.onDown.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onDown.AddListener(on_Handle_{variable.name}_Down);");
            AddCallback($"\t\tprotected virtual void on_Handle_{variable.name}_Down()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_Down.Invoke();");
            AddCallback("\t\t}");
            AddCallback("");

            AddEvent($"\t\t\t{fieldName}?.onUp.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onUp.AddListener(on_Handle_{variable.name}_Up);");
            AddCallback($"\t\tprotected virtual void on_Handle_{variable.name}_Up()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_Up.Invoke();");
            AddCallback("\t\t}");
            AddCallback("");

            AddEvent($"\t\t\t{fieldName}?.onClick.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onClick.AddListener(on_Handle_{variable.name}_Click);");
            AddCallback($"\t\tprotected virtual void on_Handle_{variable.name}_Click()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_Click.Invoke();");
            AddCallback("\t\t}");
            AddCallback("");
        }

        private void GenericButtonComponent(UIBindData VARIABLE, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent _event_{fieldName}_Click;");
            AddInit($"\t\t\t_event_{fieldName}_Click = new UnityEvent();");

            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Click(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_Click.AddListener(callback);");
            AddSetup("\t\t}");

            AddEvent($"\t\t\t{fieldName}?.onClick.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onClick.AddListener(on_Handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_Handle_{VARIABLE.name}()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_Click.Invoke();");
            AddCallback("\t\t}");
            AddCallback("");
        }

        private void GenericToggleComponent(UIBindData VARIABLE, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent<bool> _event_{fieldName}_Change;");
            AddInit($"\t\t\t_event_{fieldName}_Click = new UnityEvent();");
            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Change(UnityAction<bool> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_Change.AddListener(callback);");
            AddSetup("\t\t}");

            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_Handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_Handle_{VARIABLE.name}(bool isOn)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_Click.Invoke(isOn);");
            AddCallback("\t\t}");
            AddCallback("");
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(bool isOn)");
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
            AddField($"\t\tprivate UnityEvent<float> _event_{fieldName}_Change;");
            AddInit($"\t\t\t_event_{fieldName}_Change = new UnityEvent<float>();");
            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Change(UnityAction<float> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_Change.AddListener(callback);");
            AddSetup("\t\t}");


            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_Handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_Handle_{VARIABLE.name}(float value)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_Change.Invoke(value);");
            AddCallback("\t\t}");
            AddCallback("");

            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(float value)");
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
            AddField($"\t\tprivate UnityEvent<string> _event_{fieldName}_Submit;");
            AddInit($"\t\t\t_event_{fieldName}_Submit = new UnityEvent<string>();");
            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Submit(UnityAction<string> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_Submit.AddListener(callback);");
            AddSetup("\t\t}");

            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_Handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_Handle_{VARIABLE.name}(string value)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_Submit.Invoke(value);");
            AddCallback("\t\t}");
            AddCallback("");

            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(string value)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetTextWithoutNotify(value);");
            AddSetup("\t\t}");
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(TMP_FontAsset fontAsset)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetGlobalFontAsset(fontAsset);");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(int size)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetGlobalPointSize(size);");
            AddSetup("\t\t}");
            AddSetup("");
        }

        private void GenericDropdownComponent(UIBindData VARIABLE, string fieldName, string componentName)
        {
            AddField($"\t\tprivate UnityEvent<int> _event_{fieldName}_Change;");
            AddInit($"\t\t\t_event_{fieldName}_Change = new UnityEvent<int>();");
            AddSetup($"\t\tpublic void on_Setup_Callback_{fieldName}_Change(UnityAction<int> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_event_{fieldName}_Change.AddListener(callback);");
            AddSetup("\t\t}");

            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_Handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_Handle_{VARIABLE.name}(int value)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_event_{fieldName}_Change.Invoke(value);");
            AddCallback("\t\t}");
            AddCallback("");
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(int index)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.SetValueWithoutNotify(index);");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(List<string> items)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.AddOptions(items);");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(List<Sprite> items)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.AddOptions(items);");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(List<{componentName}.OptionData> items)");
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
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(string path)");
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
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(Sprite sprite)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.sprite = sprite;");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(Texture2D texture)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);");
            AddSetup("\t\t}");
            AddSetup("");
            if (VARIABLE.bindLanguage && VARIABLE.language != 0)
            {
                AddLanguage($"\t\t\t on_Setup_{VARIABLE.name}(Localliztion.Get({VARIABLE.language}));");
            }
        }

        private void GenericRawImageComponent(UIBindData VARIABLE, string fieldName)
        {
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(string path)");
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

            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(Sprite sprite)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.texture = sprite.texture;");
            AddSetup("\t\t}");
            AddSetup("");
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(Texture2D texture)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.texture = texture;");
            AddSetup("\t\t}");
            AddSetup("");

            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(RenderTexture texture)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.texture = texture;");
            AddSetup("\t\t}");
            AddSetup("");

            if (VARIABLE.bindLanguage && VARIABLE.language != 0)
            {
                AddLanguage($"\t\t\t on_Setup_{VARIABLE.name}(Localliztion.Get({VARIABLE.language}));");
            }
        }

        private void GenericTextComponent(UIBindData VARIABLE, string fieldName)
        {
            AddSetup($"\t\tpublic void on_Setup_{VARIABLE.name}(object info)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup("");
            AddSetup($"\t\t\t{fieldName}.text = info == null ? \"\" : info.ToString();");
            AddSetup("\t\t}");
            AddSetup("");

            if (VARIABLE.bindLanguage && VARIABLE.language != 0)
            {
                AddLanguage($"\t\t\t on_Setup_{VARIABLE.name}(Localliztion.Get({VARIABLE.language}));");
            }
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

        public string GetTempleteCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t\tpublic class Templete_{setting.NameSpace} : Templete<Templete_{setting.NameSpace}>");
            sb.AppendLine($"\t\t{{");
            fieldList.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine($"\t\t\tpublic Templete_{setting.NameSpace}(GameObject gameObject) : base(gameObject)");
            sb.AppendLine($"\t\t\t{{");
            sb.AppendLine($"\t\t\t}}");
            sb.AppendLine("");
            sb.AppendLine("\t\t\tpublic override void Awake(params object[] args)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tif(this.gameObject == null)");
            sb.AppendLine("\t\t\t\t{");
            sb.AppendLine("\t\t\t\t\treturn;");
            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\t\t\tOnBindField();");
            sb.AppendLine("\t\t\t\tOnBindEvents();");
            sb.AppendLine("\t\t\t\tOnBindLanguage();");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\t\tprivate void OnBindLanguage()");
            sb.AppendLine("\t\t\t{");
            language.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\tprivate void OnBindField()");
            sb.AppendLine("\t\t\t{");
            initList.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\t\tprivate void OnBindEvents()");
            sb.AppendLine("\t\t\t{");
            eventList.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("");
            callbackList.ForEach(x => sb.AppendLine("\t" + x));
            setupList.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine("\t\t\tpublic override void Dispose()");
            sb.AppendLine("\t\t\t{");
            disposeList.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine("\t\t\t\tbase.Dispose();");
            sb.AppendLine("\t\t\t\tGC.SuppressFinalize(this);");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
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
            templeteList.ForEach(x => sb.AppendLine(x));
            fieldList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("");
            sb.AppendLine($"\t\tpublic UIBind_{setting.name}(GameObject gameObject) : base(gameObject)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tpublic override void Awake(params object[] args)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif(this.gameObject == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\t\tOnBindField();");
            sb.AppendLine("\t\t\tOnBindEvents();");
            sb.AppendLine("\t\t\tOnBindLanguage();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprivate void OnBindLanguage()");
            sb.AppendLine("\t\t{");
            language.ForEach(x => sb.AppendLine(x));
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
            setupList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\tpublic override void Enable()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tpublic override void Disable()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tpublic override void Dispose()");
            sb.AppendLine("\t\t{");
            disposeList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t\tGC.SuppressFinalize(this);");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
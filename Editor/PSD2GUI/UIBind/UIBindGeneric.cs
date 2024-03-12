using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZGame.UI;
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
                AddField($"\t\tpublic GameObject go_{VARIABLE.name}{{ get; private set; }}");
                AddDispose($"\t\t\tgo_{VARIABLE.name} = null;");
                AddInit($"\t\t\tgo_{VARIABLE.name} = this.gameObject.transform.Find(\"{VARIABLE.path}\").gameObject;");
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
                        case nameof(Button):
                            GenericButtonComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(Toggle):
                            GenericToggleComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(Slider):
                            GenericSliderComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(TMP_InputField):
                        case nameof(InputField):
                            GenericInputFieldComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(TMP_Dropdown):
                        case nameof(Dropdown):
                            GenericDropdownComponent(VARIABLE, rule.GetFieldName(VARIABLE.name), rule.TypeName);
                            break;
                        case nameof(Image):
                            GenericImageComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(RawImage):
                            GenericRawImageComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(TextMeshProUGUI):
                        case nameof(TMP_Text):
                        case nameof(Text):
                            GenericTextComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(LongPresseButton):
                            GenericLongPressButtonComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(UISwitcher):
                            GenericUISwitcherComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(UIToolbar):
                            break;
                        case nameof(UIBind):
                            GenericTempleteComponent(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                        case nameof(LoopScrollViewer):
                            GenericLoopScrollViewerCode(VARIABLE, rule.GetFieldName(VARIABLE.name));
                            break;
                    }
                }
            }
        }


        private void GenericLoopScrollViewerCode(UIBindData variable, string fieldName)
        {
            LoopScrollViewer loopScrollViewer = setting.transform.Find(variable.path).GetComponent<LoopScrollViewer>();
            if (loopScrollViewer == null)
            {
                return;
            }

            LoopScrollCellView cellView = loopScrollViewer.prefab;
            if (cellView is null)
            {
                return;
            }
            AddEvent($"\t\t\t{fieldName}?.onCellViewRefreshed.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onCellViewRefreshed.AddListener(on_handle_{fieldName}_RefreshCellView);");
     
            AddField($"\t\tprivate UnityEvent<LoopScrollCellView> _{fieldName}_refreshEvent;");
            AddSetup($"\t\tpublic void on_setup_{fieldName}_RefreshCellViewEvent(UnityAction<LoopScrollCellView> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_refreshEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            
            AddInit($"\t\t\t_{fieldName}_refreshEvent = new UnityEvent<LoopScrollCellView>();");
            AddCallback($"\t\tprotected virtual void on_handle_{fieldName}_RefreshCellView(LoopScrollCellView value)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_refreshEvent?.Invoke(value);");
            AddCallback("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{fieldName}_DataList(params object[] args)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t{fieldName}.SetDataList(args);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
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

                AddField($"\t\tpublic Templete_{bindData.NameSpace} temp_{bindData.NameSpace}{{ get; private set; }}");
                AddInit($"\t\t\ttemp_{bindData.NameSpace} = new Templete_{bindData.NameSpace}(this.gameObject.transform.Find(\"{variable.path}\").gameObject);");
                AddDispose($"\t\t\ttemp_{bindData.NameSpace} = null;");
            }
            else
            {
                AddField($"\t\tpublic {rule.TypeName} {rule.GetFieldName(variable.name)}{{ get; private set; }}");
                AddInit($"\t\t\t{rule.GetFieldName(variable.name)} =  this.go_{variable.name}.GetComponent<{rule.TypeName}>();");
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
            AddSetup(Environment.NewLine);
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
            AddInit($"\t\t\ttemp_{bindData.NameSpace}.Disable();");
        }

        private void GenericUISwitcherComponent(UIBindData variable, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent<object> _{fieldName}_switchEvent;");
            AddInit($"\t\t\t_{fieldName}_switchEvent = new UnityEvent<object>();");

            AddSetup($"\t\tpublic void on_setup_{fieldName}_SwitchEvent(UnityAction<object> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_switchEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);

            AddEvent($"\t\t\t{fieldName}?.onSelect.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onSelect.AddListener(on_handle_{variable.name}_SwitchEvent);");
            AddCallback($"\t\tprotected virtual void on_handle_{variable.name}_SwitchEvent(object obj)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_SwitchEvent.Invoke();");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);
        }

        private void GenericLongPressButtonComponent(UIBindData variable, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent _{fieldName}_CancelEvent;");
            AddInit($"\t\t\t_{fieldName}_CancelEvent = new UnityEvent();");
            AddField($"\t\tprivate UnityEvent _{fieldName}_MouseDownEvent;");
            AddInit($"\t\t\t_{fieldName}_MouseDownEvent = new UnityEvent();");
            AddField($"\t\tprivate UnityEvent _{fieldName}_MouseUpEvent;");
            AddInit($"\t\t\t_{fieldName}_MouseUpEvent = new UnityEvent();");
            AddField($"\t\tprivate UnityEvent _{fieldName}_ClickEvent;");
            AddInit($"\t\t\t_{fieldName}_ClickEvent = new UnityEvent();");

            AddSetup($"\t\tpublic void on_setup_{fieldName}_CancelEvent(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_CancelEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{fieldName}_MouseDownEvent(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_MouseDownEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{fieldName}_MouseUpEvent(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_MouseUpEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{fieldName}_ClickEvent(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_ClickEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);

            AddEvent($"\t\t\t{fieldName}?.onCancel.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onCancel.AddListener(on_handle_{variable.name}_CancelEvent);");
            AddCallback($"\t\tprotected virtual void on_handle_{variable.name}_CancelEvent()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_CancelEvent.Invoke();");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);
            AddEvent($"\t\t\t{fieldName}?.onDown.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onDown.AddListener(on_handle_{variable.name}_MouseDownEvent);");
            AddCallback($"\t\tprotected virtual void on_handle_{variable.name}_MouseDownEvent()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_MouseDownEvent.Invoke();");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);

            AddEvent($"\t\t\t{fieldName}?.onUp.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onUp.AddListener(on_handle_{variable.name}_MouseUpEvent);");
            AddCallback($"\t\tprotected virtual void on_handle_{variable.name}_MouseUpEvent()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_MouseUpEvent.Invoke();");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);

            AddEvent($"\t\t\t{fieldName}?.onClick.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onClick.AddListener(on_handle_{variable.name}_ClickEvent);");
            AddCallback($"\t\tprotected virtual void on_handle_{variable.name}_ClickEvent()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_ClickEvent.Invoke();");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);
        }

        private void GenericButtonComponent(UIBindData VARIABLE, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent _{fieldName}_ClickEvent;");
            AddInit($"\t\t\t_{fieldName}_ClickEvent = new UnityEvent();");

            AddSetup($"\t\tpublic void on_setup_{fieldName}_ClickEvent(UnityAction callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_ClickEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddEvent($"\t\t\t{fieldName}?.onClick.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onClick.AddListener(on_handle_{VARIABLE.name}_ClickEvent);");
            AddCallback($"\t\tprotected virtual void on_handle_{VARIABLE.name}_ClickEvent()");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_ClickEvent.Invoke();");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);
        }

        private void GenericToggleComponent(UIBindData VARIABLE, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent<bool> _{fieldName}_ChangeEvent;");
            AddInit($"\t\t\t_{fieldName}_Click = new UnityEvent();");
            AddSetup($"\t\tpublic void on_setup_{fieldName}_ChangeEvent(UnityAction<bool> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_ChangeEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name}_ChangeEvent);");
            AddCallback($"\t\tprotected virtual void on_handle_{VARIABLE.name}_ChangeEvent(bool value)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_ChangeEvent.Invoke(value);");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(bool value)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.SetIsOnWithoutNotify(isOn);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
        }

        private void GenericSliderComponent(UIBindData VARIABLE, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent<float> _{fieldName}_ChangeEvent;");
            AddInit($"\t\t\t_{fieldName}_ChangeEvent = new UnityEvent<float>();");
            AddSetup($"\t\tpublic void on_setup_{fieldName}_ChangeEvent(UnityAction<float> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_ChangeEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);

            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name}_ChangeEvent);");
            AddCallback($"\t\tprotected virtual void on_handle_{VARIABLE.name}_ChangeEvent(float value)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_ChangeEvent.Invoke(value);");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);

            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(float value)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.SetValueWithoutNotify(value);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
        }

        private void GenericInputFieldComponent(UIBindData VARIABLE, string fieldName)
        {
            AddField($"\t\tprivate UnityEvent<string> _{fieldName}_SubmitEvent;");
            AddInit($"\t\t\t_{fieldName}_SubmitEvent = new UnityEvent<string>();");
            AddSetup($"\t\tpublic void on_setup_{fieldName}_SubmitEvent(UnityAction<string> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_SubmitEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_handle_{VARIABLE.name}_SubmitEvent);");
            AddCallback($"\t\tprotected virtual void on_handle_{VARIABLE.name}_SubmitEvent(string value)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_SubmitEvent.Invoke(value);");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);

            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}_TextValue(string value)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.SetTextWithoutNotify(value);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}_FontAsset(TMP_FontAsset fontAsset)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.SetGlobalFontAsset(fontAsset);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}_FontSize(int size)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.SetGlobalPointSize(size);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
        }

        private void GenericDropdownComponent(UIBindData VARIABLE, string fieldName, string componentName)
        {
            AddField($"\t\tprivate UnityEvent<int> _{fieldName}_SelectionEvent;");
            AddInit($"\t\t\t_{fieldName}_Change = new UnityEvent<int>();");
            AddSetup($"\t\tpublic void on_setup_{fieldName}_SelectionEvent(UnityAction<int> callback)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\t_{fieldName}_SelectionEvent.AddListener(callback);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.RemoveAllListeners();");
            AddEvent($"\t\t\t{fieldName}?.onValueChanged.AddListener(on_Handle_{VARIABLE.name});");
            AddCallback($"\t\tprotected virtual void on_Handle_{VARIABLE.name}(int value)");
            AddCallback("\t\t{");
            AddCallback($"\t\t\t_{fieldName}_SelectionEvent.Invoke(value);");
            AddCallback("\t\t}");
            AddCallback(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(int index)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.SetValueWithoutNotify(index);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}_DorpdownItems(List<string> items)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.AddOptions(items);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}_DorpdownItems(List<Sprite> items)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.AddOptions(items);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}_DorpdownItems(List<{componentName}.OptionData> items)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.AddOptions(items);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
        }

        private void GenericImageComponent(UIBindData VARIABLE, string fieldName)
        {
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(string path)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\tResObject handle = WorkApi.Resource.LoadAsset(path);");
            AddSetup($"\t\t\tif (handle is null || handle.IsSuccess() == false)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup($"\t\t\thandle.SetSprite({fieldName});");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(Sprite sprite)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.sprite = sprite;");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(Texture2D texture)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
        }

        private void GenericRawImageComponent(UIBindData VARIABLE, string fieldName)
        {
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(string path)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\tResObject handle = WorkApi.Resource.LoadAsset(path);");
            AddSetup($"\t\t\tif (handle is null || handle.IsSuccess() == false)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup($"\t\t\thandle.SetTexture2D({fieldName});");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);

            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(Sprite sprite)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.texture = sprite.texture;");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(Texture2D texture)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.texture = texture;");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);

            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(RenderTexture texture)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.texture = texture;");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
        }

        private void GenericTextComponent(UIBindData VARIABLE, string fieldName)
        {
            AddSetup($"\t\tpublic void on_setup_{VARIABLE.name}(object info)");
            AddSetup("\t\t{");
            AddSetup($"\t\t\tif ({fieldName}== null)");
            AddSetup($"\t\t\t\treturn;");
            AddSetup(Environment.NewLine);
            AddSetup($"\t\t\t{fieldName}.text = info == null ? \"\" : info.ToString();");
            AddSetup("\t\t}");
            AddSetup(Environment.NewLine);
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

            sb.AppendLine("namespace " + (setting.NameSpace.IsNullOrEmpty() ? "UICode" : setting.NameSpace));
            sb.AppendLine("{");


            sb.AppendLine("\t/// <summary>");
            sb.AppendLine("\t/// Createing with " + DateTime.Now.ToString("g"));
            sb.AppendLine("\t/// by " + SystemInfo.deviceName);
            sb.AppendLine("\t/// </summary>");
            string temp = AssetDatabase.GetAssetPath(setting);
            Debug.Log("Reference Resource Path:" + temp);
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
            int index = 0;
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\tpublic override void Awake()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Awake();");
            sb.AppendLine("\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\tpublic override void Enable(params object[] args)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Enable(args);");
            sb.AppendLine("\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\tpublic override void Disable()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Disable();");
            sb.AppendLine("\t\t}");
            sb.AppendLine(Environment.NewLine);
            callbackList.ForEach(x =>
            {
                if (index % 5 != 2)
                {
                    sb.AppendLine(x.Replace("virtual", "override"));
                }

                index++;
            });
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string GetTempleteCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\t/// <summary>");
            sb.AppendLine("\t/// Secondary file generated by UIBind tool");
            sb.AppendLine("\t/// Please do not modify this file as it will be overwritten");
            sb.AppendLine("\t/// By:" + SystemInfo.deviceName);
            sb.AppendLine("\t/// Time: " + DateTime.Now.ToString("g"));
            sb.AppendLine("\t/// </summary>");
            sb.AppendLine($"\t\tpublic class Templete_{setting.NameSpace} : Templete<Templete_{setting.NameSpace}>");
            sb.AppendLine($"\t\t{{");
            fieldList.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine($"\t\t\tpublic Templete_{setting.NameSpace}(GameObject gameObject) : base(gameObject)");
            sb.AppendLine($"\t\t\t{{");
            sb.AppendLine($"\t\t\t}}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\t\tpublic override void Awake()");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tif(this.gameObject == null)");
            sb.AppendLine("\t\t\t\t{");
            sb.AppendLine("\t\t\t\t\treturn;");
            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\t\t\tOnBindField();");
            sb.AppendLine("\t\t\t\tOnBindEvents();");
            sb.AppendLine("\t\t\t\tOnBindLanguage();");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\t\tprivate void OnBindLanguage()");
            sb.AppendLine("\t\t\t{");
            language.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine("\t\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\t\tprivate void OnBindField()");
            sb.AppendLine("\t\t\t{");
            initList.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine("\t\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\t\tprivate void OnBindEvents()");
            sb.AppendLine("\t\t\t{");
            eventList.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine("\t\t\t}");
            sb.AppendLine(Environment.NewLine);
            callbackList.ForEach(x => sb.AppendLine("\t" + x));
            setupList.ForEach(x => sb.AppendLine("\t" + x));
            sb.AppendLine(Environment.NewLine);
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
            sb.AppendLine("namespace " + (setting.NameSpace.IsNullOrEmpty() ? "UICode" : setting.NameSpace));
            sb.AppendLine("{");
            sb.AppendLine("\t/// <summary>");
            sb.AppendLine("\t/// Secondary file generated by UIBind tool");
            sb.AppendLine("\t/// Please do not modify this file as it will be overwritten");
            sb.AppendLine("\t/// By:" + SystemInfo.deviceName);
            sb.AppendLine("\t/// Time: " + DateTime.Now.ToString("g"));
            sb.AppendLine("\t/// </summary>");
            sb.AppendLine("\tpublic class UIBind_" + setting.name + " : UIBase");
            sb.AppendLine("\t{");
            templeteList.ForEach(x => sb.AppendLine(x));
            fieldList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine(Environment.NewLine);

            sb.AppendLine($"\t\tpublic UIBind_{setting.name}(GameObject gameObject) : base(gameObject)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\tpublic override void Awake()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif(this.gameObject == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\t\tOnBindField();");
            sb.AppendLine("\t\t\tOnBindEvents();");
            sb.AppendLine("\t\t\tOnBindLanguage();");
            sb.AppendLine("\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\tprivate void OnBindLanguage()");
            sb.AppendLine("\t\t{");
            language.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\tprivate void OnBindField()");
            sb.AppendLine("\t\t{");
            initList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t}");
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("\t\tprivate void OnBindEvents()");
            sb.AppendLine("\t\t{");
            eventList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t}");
            sb.AppendLine(Environment.NewLine);
            callbackList.ForEach(x => sb.AppendLine(x));
            setupList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\tpublic override void Dispose()");
            sb.AppendLine("\t\t{");
            disposeList.ForEach(x => sb.AppendLine(x));
            sb.AppendLine("\t\t\tbase.Dispose();");
            sb.AppendLine("\t\t\tGC.SuppressFinalize(this);");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
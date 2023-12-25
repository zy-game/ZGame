using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Window;

namespace ZGame.Editor.UIBindEditor
{
    [ResourceReference("Assets/Settings/UIBindRuler.asset")]
    public class UIBindRulerConfig : SingletonScriptableObject<UIBindRulerConfig>
    {
        public List<UIBindRulerItem> rules;
        public List<string> nameSpaces;

        protected override void OnAwake()
        {
            if (rules is null)
            {
                rules = new List<UIBindRulerItem>();
            }

            if (nameSpaces is null)
            {
                nameSpaces = new List<string>();
            }
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


        public void GenericUIBindCode(UIBind setting)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using ZGame.Window;");
            sb.AppendLine("using System;");
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Createing with " + DateTime.Now.ToString("g"));
            sb.AppendLine("/// by " + SystemInfo.deviceName);
            sb.AppendLine("/// </summary>");
            sb.AppendLine("namespace " + setting.NameSpace);
            sb.AppendLine("{");
            sb.AppendLine("\tpublic class UI_" + setting.name + " : UIBase");
            sb.AppendLine("\t{");
            foreach (var VARIABLE in setting.options)
            {
                sb.AppendLine($"\t\tpublic GameObject gameObject_{VARIABLE.name};");
                foreach (var VARIABLE2 in VARIABLE.selector.items)
                {
                    if (VARIABLE2.isOn is false)
                    {
                        continue;
                    }

                    UIBindRulerItem rulerItem = UIBindRulerConfig.instance.GetRule(VARIABLE2.name);
                    if (rulerItem is null)
                    {
                        continue;
                    }

                    string[] ts = VARIABLE2.name.Split('.');
                    string componentName = ts[ts.Length - 1];
                    sb.AppendLine($"\t\tpublic {rulerItem.fullName} {componentName}_{VARIABLE.name};");
                }
            }

            sb.AppendLine("");
            sb.AppendLine($"\t\tpublic UI_{setting.name}(GameObject gameObject) : base(gameObject)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tpublic override void Awake()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tOnBindComponents();");
            sb.AppendLine("\t\t\tOnBindEvents();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprotected virtual void OnBindComponents()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif(this.gameObject == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn;");
            sb.AppendLine("\t\t\t}");
            foreach (var VARIABLE in setting.options)
            {
                sb.AppendLine($"\t\t\tgameObject_{VARIABLE.name} = this.gameObject.transform.Find(\"{VARIABLE.path}\").gameObject;");
                foreach (var VARIABLE2 in VARIABLE.selector.items)
                {
                    if (VARIABLE2.isOn is false)
                    {
                        continue;
                    }

                    UIBindRulerItem rulerItem = UIBindRulerConfig.instance.GetRule(VARIABLE2.name);
                    if (rulerItem is null)
                    {
                        continue;
                    }

                    string[] ts = VARIABLE2.name.Split('.');
                    string componentName = ts[ts.Length - 1];
                    sb.AppendLine($"\t\t\t{componentName}_{VARIABLE.name} = this.gameObject_{VARIABLE.name}.GetComponent<{componentName}>();");
                }
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprotected virtual void OnBindEvents()");
            sb.AppendLine("\t\t{");
            foreach (var VARIABLE in setting.options)
            {
                foreach (var VARIABLE2 in VARIABLE.selector.items)
                {
                    if (VARIABLE2.isOn is false)
                    {
                        continue;
                    }

                    string[] ts = VARIABLE2.name.Split('.');
                    string name = $"{ts[ts.Length - 1]}_{VARIABLE.name}";
                    Type componentType = Type.GetType(VARIABLE2.name);
                    Debug.Log(VARIABLE2.name + "  " + componentType);

                    if (componentType == typeof(Button)) //|| VARIABLE2.name.EndsWith("Button"))
                    {
                        sb.AppendLine($"\t\t\t{name}?.onClick.RemoveAllListeners();");
                        sb.AppendLine($"\t\t\t{name}?.onClick.AddListener(on_handle_{name});");
                    }
                    else if (componentType == typeof(Toggle))
                    {
                        sb.AppendLine($"\t\t\t{name}?.onValueChanged.RemoveAllListeners();");
                        sb.AppendLine($"\t\t\t{name}?.onValueChanged.AddListener(on_handle_{name});");
                    }
                    else if (componentType == typeof(Slider))
                    {
                        sb.AppendLine($"\t\t\t{name}?.onValueChanged.RemoveAllListeners();");
                        sb.AppendLine($"\t\t\t{name}?.onValueChanged.AddListener(on_handle_{name});");
                    }
                    else if (componentType == typeof(TMP_InputField))
                    {
                        sb.AppendLine($"\t\t\t{name}?.onEndEdit.RemoveAllListeners();");
                        sb.AppendLine($"\t\t\t{name}?.onEndEdit.AddListener(on_handle_{name});");
                    }
                }
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            foreach (var VARIABLE in setting.options)
            {
                foreach (var VARIABLE2 in VARIABLE.selector.items)
                {
                    string[] ts = VARIABLE2.name.Split('.');
                    string name = $"{ts[ts.Length - 1]}_{VARIABLE.name}";
                    Type componentType = Type.GetType(VARIABLE2.name);

                    bool m = false;
                    if (componentType == typeof(Button)) //|| VARIABLE2.name.EndsWith("Button"))
                    {
                        sb.AppendLine("\t\tprotected virtual void on_handle_{name}()");
                        m = true;
                    }
                    else if (componentType == typeof(Toggle))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_handle_{name}(bool isOn)");
                        m = true;
                    }
                    else if (componentType == typeof(Slider))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_handle_{name}(float value)");
                        m = true;
                    }
                    else if (componentType == typeof(TMP_InputField))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_handle_{name}(string value)");
                        m = true;
                    }

                    if (m)
                    {
                        sb.AppendLine("\t\t{");
                        sb.AppendLine("\t\t}");
                        sb.AppendLine("");
                    }
                }
            }

            sb.AppendLine("");
            foreach (var VARIABLE in setting.options)
            {
                foreach (var VARIABLE2 in VARIABLE.selector.items)
                {
                    string[] ts = VARIABLE2.name.Split('.');
                    string name = $"{ts[ts.Length - 1]}_{VARIABLE.name}";
                    Type componentType = Type.GetType(VARIABLE2.name);
                    if (componentType == typeof(Toggle))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_setup_{name}(bool isOn)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\t{name}?.isOn = isOn;");
                        sb.AppendLine("\t\t}");
                    }
                    else if (componentType == typeof(Slider))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_setup_{name}(float value)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\t{name}?.value = value;");
                        sb.AppendLine("\t\t}");
                    }
                    else if (componentType == typeof(TMP_InputField))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_setup_{name}(string value)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\t{name}?.text = value;");
                        sb.AppendLine("\t\t}");
                    }
                    else if (componentType == typeof(Image))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_setup_{name}(Sprite sprite)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\t{name}?.sprite = sprite;");
                        sb.AppendLine("\t\t}");
                        sb.AppendLine($"\t\tprotected virtual void on_setup_{name}(Texture2D texture)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\t{name}?.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);");
                        sb.AppendLine("\t\t}");
                    }
                    else if (componentType == typeof(RawImage))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_setup_{name}(Sprite sprite)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\t{name}?.texture = sprite.texture;");
                        sb.AppendLine("\t\t}");
                        sb.AppendLine($"\t\tprotected virtual void on_setup_{name}(Texture2D texture)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\t{name}?.texture = texture;");
                        sb.AppendLine("\t\t}");
                        sb.AppendLine($"\t\tprotected virtual void on_setup_{name}(RendererTexture texture)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\t{name}?.texture = texture;");
                        sb.AppendLine("\t\t}");
                    }
                    else if (componentType == typeof(Text))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_setup_{name}(string text)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\t{name}?.text = text;");
                        sb.AppendLine("\t\t}");
                    }
                }
            }

            sb.AppendLine("\t\tpublic override void Dispose()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Dispose();");
            foreach (var VARIABLE in setting.options)
            {
                foreach (var VARIABLE2 in VARIABLE.selector.items)
                {
                    string[] ts = VARIABLE2.name.Split('.');
                    string componentName = ts[ts.Length - 1];
                    sb.AppendLine($"\t\t\t{componentName}_{VARIABLE.name} = null;");
                }
            }

            sb.AppendLine("\t\t}");
            // sb.AppendLine("");
            // sb.AppendLine("\t\tpublic virtual void OnRefresh()");
            // sb.AppendLine("\t\t{");
            // sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string path = AssetDatabase.GetAssetPath(setting.output);
            if (path.IsNullOrEmpty())
            {
                return;
            }

            File.WriteAllText($"{path}/UI_{setting.name}.cs", sb.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(setting.name + " Generid UICode Finishing");
        }
    }


    [Serializable]
    public class UIBindRulerItem
    {
        public string fullName;
        public string prefix;

        public string GetFileName()
        {
            return fullName.Split('.')[0];
        }
    }
}
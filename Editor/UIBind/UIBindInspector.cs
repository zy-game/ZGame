using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Window;

namespace ZGame.Editor.UIBind
{
    [CustomEditor(typeof(UIBindSetting))]
    public class UIBindInspector : UnityEditor.Editor
    {
        private UIBindSetting setting;

        public void OnEnable()
        {
            this.setting = (UIBindSetting)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            setting.NameSpace = EditorGUILayout.TextField("NameSpace", setting.NameSpace);

            EditorGUILayout.BeginHorizontal();
            setting.output = EditorGUILayout.ObjectField("Output", setting.output, typeof(DefaultAsset));
            EditorGUILayout.EndHorizontal();
            this.BeginColor(ZStyle.inColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1));
            this.EndColor();


            if (setting.options == null)
            {
                setting.options = new List<BindOptions>();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Bind List:", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("", ZStyle.GUI_STYLE_ADD_BUTTON))
                {
                    setting.options.Add(new BindOptions());
                }

                GUILayout.EndHorizontal();
            }

            if (setting.options.Count == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Not Data List", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }


            for (int i = setting.options.Count - 1; i >= 0; i--)
            {
                OnDrawingBindItemData(setting.options[i]);
            }

            if (EditorGUI.EndChangeCheck())
            {
                this.SaveChanges();
                EditorUtility.SetDirty(setting);
            }

            GUILayout.EndVertical();

            if (GUILayout.Button("Generic"))
            {
                OnGenericCode();
            }
        }

        private void OnDrawingBindItemData(BindOptions options)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (options.target == null)
            {
                if (options.path.IsNullOrEmpty() is false)
                {
                    options.target = setting.transform.Find(options.path)?.gameObject;
                }
            }

            GUILayout.Label(options.name);
            options.target = (GameObject)EditorGUILayout.ObjectField(options.target, typeof(GameObject), true);

            List<string> items = new List<string>();
            if (options.target != null)
            {
                Component[] opComs = options.target.GetComponents<Component>();
                foreach (var VARIABLE in opComs)
                {
                    if (setting.nameSpace.Contains(VARIABLE.GetType().Namespace))
                    {
                        continue;
                    }

                    setting.nameSpace.Add(VARIABLE.GetType().Namespace);
                }

                items.AddRange(opComs.Select(x => x.GetType().FullName));

                string name = String.Empty;
                if (options.type.Count == 0)
                {
                    name = "Noting";
                }
                else
                {
                    if (items.Count == options.type.Count)
                    {
                        name = "Everyting";
                    }
                    else
                    {
                        name = string.Join(",", items);
                        if (name.Length > 20)
                        {
                            name = name.Substring(0, 25) + "...";
                        }
                    }
                }

                if (GUILayout.Button(name, EditorStyles.popup))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Noting"), options.type.Count == 0, () => { options.type.Clear(); });
                    menu.AddItem(new GUIContent("Everything"), items.Count == options.type.Count, () =>
                    {
                        options.type.Clear();
                        options.type.AddRange(items);
                    });
                    foreach (var VARIABLE in items)
                    {
                        menu.AddItem(new GUIContent(VARIABLE), options.type.Contains(VARIABLE), () =>
                        {
                            if (options.type.Contains(VARIABLE))
                            {
                                options.type.Remove(VARIABLE);
                            }
                            else
                            {
                                options.type.Add(VARIABLE);
                            }
                        });
                    }

                    menu.ShowAsContext();
                }

                options.path = GetPath(options.target.transform);
                options.name = options.target.name.Replace(" ", "(", ")");
            }

            // if (options.type != "None")
            // {
            //     Type type = AppDomain.CurrentDomain.GetTypeForThat(options.type);
            //     var click = type.GetProperty("onClick");
            //     var change = type.GetProperty("onValueChanged");
            //     if (click != null || change != null)
            //     {
            //         options.overrideEventCode = GUILayout.Toggle(options.overrideEventCode, "Event Code");
            //     }
            // }

            if (options.type.Find(x => x.EndsWith("TextMeshProUGUI")) != null)
            {
                options.language = EditorGUILayout.IntField("", options.language, GUILayout.Width(100));
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
            {
                setting.options.Remove(options);
                EditorUtility.SetDirty(setting);
                this.Repaint();
            }

            GUILayout.EndHorizontal();
        }

        private string GetPath(Transform _transform)
        {
            string path = "";
            while (_transform.GetComponent<UIBindSetting>() == null)
            {
                if (_transform.parent == null)
                {
                    break;
                }

                path = $"{_transform.name}/{path}";
                _transform = _transform.parent;
            }

            path = path.Substring(0, path.Length - 1);
            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGenericCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using ZGame.Window;");
            sb.AppendLine("using System;");
            setting.nameSpace.ForEach(x => sb.AppendLine($"using {x};"));
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Createing with " + DateTime.Now.ToString("g"));
            sb.AppendLine("/// by " + SystemInfo.deviceName);
            sb.AppendLine("/// </summary>");
            sb.AppendLine("namespace " + setting.NameSpace);
            sb.AppendLine("{");
            sb.AppendLine("\tpublic abstract class UI_Bind_" + setting.name + " : GameWindow");
            sb.AppendLine("\t{");

            foreach (var VARIABLE in setting.options)
            {
                foreach (var VARIABLE2 in VARIABLE.type)
                {
                    string[] ts = VARIABLE2.Split('.');
                    sb.AppendLine($"\t\tpublic UIBind<{ts[ts.Length - 1]}> {ts[ts.Length - 1]}_{VARIABLE.name};");
                }
            }

            sb.AppendLine("\n");
            sb.AppendLine("\t\tpublic override void Awake()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tOnBind();");
            sb.AppendLine("\t\t\tOnEventRegister();");
            sb.AppendLine("\t\t\tOnRefresh();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprivate void OnBind()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif(this.gameObject == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn;");
            sb.AppendLine("\t\t\t}");
            foreach (var VARIABLE in setting.options)
            {
                foreach (var VARIABLE2 in VARIABLE.type)
                {
                    string[] ts = VARIABLE2.Split('.');
                    sb.AppendLine($"\t\t\t{ts[ts.Length - 1]}_{VARIABLE.name} = new UIBind<{ts[ts.Length - 1]}>(this.gameObject.transform.Find(\"{VARIABLE.path}\"));");
                }
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprivate void OnEventRegister()");
            sb.AppendLine("\t\t{");
            foreach (var VARIABLE in setting.options)
            {
                foreach (var VARIABLE2 in VARIABLE.type)
                {
                    string[] ts = VARIABLE2.Split('.');
                    string name = $"{ts[ts.Length - 1]}_{VARIABLE.name}";
                    if (VARIABLE2.EndsWith("Button"))
                    {
                        sb.AppendLine($"\t\t\t{name}?.SetCallback(new Action(on_invoke_ButtonEvent_{name}));");
                    }
                    else if (VARIABLE2.EndsWith("Toggle"))
                    {
                        sb.AppendLine($"\t\t\t{name}?.SetCallback(new Action<bool>(on_invoke_ValueChangeEvent_{name}));");
                    }
                    else if (VARIABLE2.EndsWith("Slider"))
                    {
                        sb.AppendLine($"\t\t\t{name}?.SetCallback(new Action<float>(on_invoke_ValueChangeEvent_{name}));");
                    }
                    else if (VARIABLE2.EndsWith("InputField"))
                    {
                        sb.AppendLine($"\t\t\t{name}?.SetCallback(new Action<string>(on_invoke_ValueChangeEvent_{name}));");
                    }
                }
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            foreach (var VARIABLE in setting.options)
            {
                foreach (var VARIABLE2 in VARIABLE.type)
                {
                    string[] ts = VARIABLE2.Split('.');
                    string name = $"{ts[ts.Length - 1]}_{VARIABLE.name}";
                    bool m = false;
                    if (VARIABLE2.EndsWith("Button"))
                    {
                        sb.AppendLine("\t\tprotected virtual void on_invoke_ButtonEvent_{name}()");
                        m = true;
                    }
                    else if (VARIABLE2.EndsWith("Toggle"))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_invoke_ValueChangeEvent_{name}(bool isOn)");
                        m = true;
                    }
                    else if (VARIABLE2.EndsWith("Slider"))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_invoke_ValueChangeEvent_{name}(float value)");
                        m = true;
                    }
                    else if (VARIABLE2.EndsWith("InputField"))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_invoke_ValueChangeEvent_{name}(string value)");
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


            sb.AppendLine("\t\tpublic void OnRefresh()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string path = AssetDatabase.GetAssetPath(setting.output);
            if (path.IsNullOrEmpty())
            {
                return;
            }

            File.WriteAllText($"{path}/UI_Bind_{setting.name}.cs", sb.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
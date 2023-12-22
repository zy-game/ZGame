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
    [CustomEditor(typeof(UIBindEditor))]
    public class UIBindInspector : UnityEditor.Editor
    {
        private UIBindEditor setting;


        public void OnEnable()
        {
            this.setting = (UIBindEditor)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            setting.BindConfig.NameSpace = EditorGUILayout.TextField("NameSpace", setting.BindConfig.NameSpace);

            EditorGUILayout.BeginHorizontal();
            setting.BindConfig.output = EditorGUILayout.ObjectField("Output", setting.BindConfig.output, typeof(DefaultAsset), false);
            EditorGUILayout.EndHorizontal();
            this.BeginColor(ZStyle.inColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1));
            this.EndColor();


            if (setting.BindConfig.options == null)
            {
                setting.BindConfig.options = new List<UIBindData>();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Bind List:", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("", ZStyle.GUI_STYLE_ADD_BUTTON))
                {
                    setting.BindConfig.options.Add(new UIBindData());
                }

                GUILayout.EndHorizontal();
            }

            if (setting.BindConfig.options.Count == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Not Data List", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }


            for (int i = setting.BindConfig.options.Count - 1; i >= 0; i--)
            {
                OnDrawingBindItemData(setting.BindConfig.options[i]);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(setting);
            }

            GUILayout.EndVertical();

            if (GUILayout.Button("Generic"))
            {
                OnGenericCode();
            }
        }

        private void OnDrawingBindItemData(UIBindData options)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (options.target == null)
            {
                if (options.path.IsNullOrEmpty() is false)
                {
                    options.target = setting.transform.Find(options.path)?.gameObject;
                }
            }

            options.target = (GameObject)EditorGUILayout.ObjectField(options.target, typeof(GameObject), true);

            List<string> items = new List<string>();
            if (options.target != null)
            {
                options.path = GetPath(options.target.transform);
                if (options.name.IsNullOrEmpty())
                {
                    options.name = options.target.name.Replace(" ", "(", ")");
                }

                options.name = EditorGUILayout.TextField(options.name);

                Component[] opComs = options.target.GetComponents<Component>();
                foreach (var VARIABLE in opComs)
                {
                    if (setting.BindConfig.reference.Contains(VARIABLE.GetType().Namespace))
                    {
                        continue;
                    }

                    setting.BindConfig.reference.Add(VARIABLE.GetType().Namespace);
                }

                items.AddRange(opComs.Select(x => x.GetType().FullName));

                if (GUILayout.Button(options.selector.ToString(), EditorStyles.popup))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Noting"), options.selector.isNone, () => { options.selector.Clear(); });
                    menu.AddItem(new GUIContent("Everything"), options.selector.isAll, () => { options.selector.SelectAll(); });
                    foreach (var VARIABLE in items)
                    {
                        menu.AddItem(new GUIContent(VARIABLE), options.selector.IsSelected(VARIABLE), () =>
                        {
                            if (options.selector.IsSelected(VARIABLE))
                            {
                                options.selector.UnSelect(VARIABLE);
                            }
                            else
                            {
                                options.selector.Select(VARIABLE);
                            }
                        });
                    }

                    menu.ShowAsContext();
                }
            }

            options.language = EditorGUILayout.IntField(options.language);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
            {
                setting.BindConfig.options.Remove(options);
                EditorUtility.SetDirty(setting);
                this.Repaint();
            }

            GUILayout.EndHorizontal();
        }

        private string GetPath(Transform _transform)
        {
            string path = "";
            while (_transform.GetComponent<UIBindEditor>() == null)
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
            setting.BindConfig.reference.ForEach(x => sb.AppendLine($"using {x};"));
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Createing with " + DateTime.Now.ToString("g"));
            sb.AppendLine("/// by " + SystemInfo.deviceName);
            sb.AppendLine("/// </summary>");
            sb.AppendLine("namespace " + setting.BindConfig.NameSpace);
            sb.AppendLine("{");
            sb.AppendLine("\tpublic class UI_" + setting.name + " : UIBase");
            sb.AppendLine("\t{");

            foreach (var VARIABLE in setting.BindConfig.options)
            {
                foreach (var VARIABLE2 in VARIABLE.selector.reference)
                {
                    string[] ts = VARIABLE2.Split('.');
                    sb.AppendLine($"\t\tpublic UIBind<{ts[ts.Length - 1]}> {ts[ts.Length - 1]}_{VARIABLE.name};");
                }
            }

            sb.AppendLine("");
            sb.AppendLine($"\t\tpublic UI_{setting.name}(GameObject gameObject) : base(gameObject)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tpublic override void Awake()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tOnBind();");
            sb.AppendLine("\t\t\tOnEventRegister();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprotected virtual void OnBind()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif(this.gameObject == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn;");
            sb.AppendLine("\t\t\t}");
            foreach (var VARIABLE in setting.BindConfig.options)
            {
                foreach (var VARIABLE2 in VARIABLE.selector.reference)
                {
                    string[] ts = VARIABLE2.Split('.');
                    sb.AppendLine($"\t\t\t{ts[ts.Length - 1]}_{VARIABLE.name} = new UIBind<{ts[ts.Length - 1]}>(this.gameObject.transform.Find(\"{VARIABLE.path}\"));");
                }
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tprotected virtual void OnEventRegister()");
            sb.AppendLine("\t\t{");
            foreach (var VARIABLE in setting.BindConfig.options)
            {
                foreach (var VARIABLE2 in VARIABLE.selector.reference)
                {
                    string[] ts = VARIABLE2.Split('.');
                    string name = $"{ts[ts.Length - 1]}_{VARIABLE.name}";
                    if (VARIABLE2.EndsWith("Button"))
                    {
                        sb.AppendLine($"\t\t\t{name}?.Setup(new Action(on_invoke_{name}));");
                    }
                    else if (VARIABLE2.EndsWith("Toggle"))
                    {
                        sb.AppendLine($"\t\t\t{name}?.Setup(new Action<bool>(on_invoke_{name}));");
                    }
                    else if (VARIABLE2.EndsWith("Slider"))
                    {
                        sb.AppendLine($"\t\t\t{name}?.Setup(new Action<float>(on_invoke_{name}));");
                    }
                    else if (VARIABLE2.EndsWith("InputField"))
                    {
                        sb.AppendLine($"\t\t\t{name}?.Setup(new Action<string>(on_invoke_{name}));");
                    }
                }
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            foreach (var VARIABLE in setting.BindConfig.options)
            {
                foreach (var VARIABLE2 in VARIABLE.selector.reference)
                {
                    string[] ts = VARIABLE2.Split('.');
                    string name = $"{ts[ts.Length - 1]}_{VARIABLE.name}";
                    bool m = false;
                    if (VARIABLE2.EndsWith("Button"))
                    {
                        sb.AppendLine("\t\tprotected virtual void on_invoke_{name}()");
                        m = true;
                    }
                    else if (VARIABLE2.EndsWith("Toggle"))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_invoke_{name}(bool isOn)");
                        m = true;
                    }
                    else if (VARIABLE2.EndsWith("Slider"))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_invoke_{name}(float value)");
                        m = true;
                    }
                    else if (VARIABLE2.EndsWith("InputField"))
                    {
                        sb.AppendLine($"\t\tprotected virtual void on_invoke_{name}(string value)");
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

            sb.AppendLine("\t\tpublic override void Dispose()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Dispose();");
            foreach (var VARIABLE in setting.BindConfig.options)
            {
                foreach (var VARIABLE2 in VARIABLE.selector.reference)
                {
                    string[] ts = VARIABLE2.Split('.');
                    sb.AppendLine($"\t\t\t{ts[ts.Length - 1]}_{VARIABLE.name}?.Dispose();");
                }
            }

            sb.AppendLine("\t\t}");
            // sb.AppendLine("");
            // sb.AppendLine("\t\tpublic virtual void OnRefresh()");
            // sb.AppendLine("\t\t{");
            // sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string path = AssetDatabase.GetAssetPath(setting.BindConfig.output);
            if (path.IsNullOrEmpty())
            {
                return;
            }

            File.WriteAllText($"{path}/UI_{setting.name}.cs", sb.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
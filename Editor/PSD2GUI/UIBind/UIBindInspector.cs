using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Config;
using ZGame.Editor.ResBuild.Config;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Editor.PSD2GUI
{
    [CustomEditor(typeof(UIBind))]
    public class UIBindInspector : CustomEditorWindow
    {
        private UIBind setting;
        private bool isSetLanguage = false;

        public void OnEnable()
        {
            this.setting = (UIBind)target;
            this.setting.templetee = setting.transform.parent != null && setting.transform.parent.GetComponentInParent<UIBind>() != null;
        }


        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            setting.NameSpace = EditorGUILayout.TextField(this.setting.templetee ? "Template Name" : "NameSpace", setting.NameSpace);
            if (this.setting.templetee is false)
            {
                setting.language = (TextAsset)EditorGUILayout.ObjectField("Language", setting.language, typeof(TextAsset), false);
                if (setting.language != null && isSetLanguage is false)
                {
                    Localliztion.SetupText(setting.language.text);
                    isSetLanguage = true;
                    Localliztion.Switch(BasicConfig.instance.curEntry.language);
                }

                EditorGUILayout.BeginHorizontal();
                setting.output = EditorGUILayout.ObjectField("Output", setting.output, typeof(DefaultAsset), false);
                if (EditorGUILayout.DropdownButton(new GUIContent("Generic"), FocusType.Passive, GUILayout.Width(70)))
                {
                    if (setting.output == null)
                    {
                        EditorUtility.DisplayDialog("Error", "Please select output path", "OK");
                        return;
                    }

                    if (setting.options.Count == 0)
                    {
                        EditorUtility.DisplayDialog("Error", "Please select bind list", "OK");
                        return;
                    }

                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Generic UIBind"), false, () => { UIBindRulerConfig.instance.GenericUIBindCode(setting, false); });
                    menu.AddItem(new GUIContent("Generic UIBind And UICode"), false, () => { UIBindRulerConfig.instance.GenericUIBindCode(setting, true); });
                    menu.ShowAsContext();
                }

                EditorGUILayout.EndHorizontal();
            }

            this.BeginColor(ZStyle.inColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1));
            this.EndColor();


            if (setting.options == null)
            {
                setting.options = new List<UIBindData>();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Bind List:", EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    setting.options.Clear();
                }

                GUILayout.BeginVertical();
                GUILayout.Space(-1);
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.REFRESH_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    OnRefreshBindData();
                }

                GUILayout.EndVertical();

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    setting.options.Add(new UIBindData());
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
                UIBindData options = setting.options[i];
                if (options.target == null)
                {
                    if (options.path.IsNullOrEmpty() is false)
                    {
                        options.target = setting.transform.Find(options.path)?.gameObject;
                    }
                }

                GUILayout.BeginVertical(ZStyle.ITEM_BACKGROUND_STYLE);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                options.isOn = EditorGUILayout.Foldout(options.isOn, "");
                GUILayout.Space(-50);
                Object selection = EditorGUILayout.ObjectField(options.target, typeof(Object), true);
                if (selection != null)
                {
                    GameObject target = selection as GameObject;
                    if (options.target == null)
                    {
                        options.target = target;
                        Debug.Log("初始化");
                        OnDrawingBindItemData(options);
                        EditorUtility.SetDirty(setting);
                    }
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    setting.options.Remove(options);
                    EditorUtility.SetDirty(setting);
                    this.Repaint();
                }

                GUILayout.EndHorizontal();
                if (options.target != null)
                {
                    options.name = options.target.name;
                }

                options.name = options.name?.Replace(" ", "(", ")");

                if (options.isOn)
                {
                    OnDrawingBindItemData(setting.options[i]);
                }

                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(setting);
            }

            GUILayout.EndVertical();
        }

        private bool GetPath(Transform _transform, out string path)
        {
            if (_transform.Equals(setting.transform))
            {
                path = String.Empty;
                return false;
            }

            path = String.Empty;
            while (_transform.Equals(setting.transform) is false)
            {
                if (_transform.parent is null)
                {
                    break;
                }

                path = _transform.name + "/" + path;
                _transform = _transform.parent;
            }

            path = path.Substring(0, path.Length - 1);
            return true;
        }

        private void OnRefreshBindData()
        {
            RectTransform[] rectTransforms = setting.GetComponentsInChildren<RectTransform>();
            foreach (var VARIABLE in rectTransforms)
            {
                if (GetPath(VARIABLE.transform, out string path) is false)
                {
                    continue;
                }

                if (setting.options.Exists(x => x.path == path) || setting.options.Exists(x => x.name == VARIABLE.name))
                {
                    continue;
                }

                UIBindData data = new UIBindData();
                data.target = VARIABLE.gameObject;
                data.name = VARIABLE.name;
                data.path = path;
                data.selector = new Selector();
                data.selector.Add(VARIABLE.GetComponents(typeof(Component)).Select(x => x.GetType().FullName).ToArray());
                data.selector.items.ForEach(x =>
                {
                    if (UIBindRulerConfig.instance.TryGetRuler(x.name, out var rulerItem) is false)
                    {
                        return;
                    }

                    x.isOn = VARIABLE.name.StartsWith(rulerItem.prefix);
                });
                setting.options.Add(data);
            }
        }

        private void OnDrawingBindItemData(UIBindData options)
        {
            if (options.target == null)
            {
                return;
            }

            GUILayout.Space(10);
            GetPath(options.target.transform, out options.path);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Path", GUILayout.Width(110));
            GUILayout.Label(options.path);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Bind Components", GUILayout.Width(110));
            if (EditorGUILayout.DropdownButton(new GUIContent(options.selector.ToString()), FocusType.Passive))
            {
                options.selector.ShowContext(() => { EditorUtility.SetDirty(setting); });
            }

            GUILayout.EndHorizontal();
            List<Component> opComs = options.target.GetComponents<Component>().ToList();
            if (options.selector is null)
            {
                options.selector = new Selector();
            }


            options.selector.Add(opComs.Select(x => x.GetType().FullName).ToArray());

            if (options.isText is false)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Language", GUILayout.Width(110));
            options.bindLanguage = GUILayout.Toggle(options.bindLanguage, "");
            GUILayout.EndHorizontal();

            if (options.bindLanguage is false)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Bind", GUILayout.Width(110));
            List<string> languageData = Localliztion.GetValues();
            if (languageData is null || languageData.Count == 0)
            {
                GUILayout.EndHorizontal();
                return;
            }

            SetLanguage(options);
            if (EditorGUILayout.DropdownButton(new GUIContent(Localliztion.Get(options.language)), FocusType.Passive))
            {
                ObjectSelectionWindow<string>.ShowSingle(new Vector2(200, 300), languageData, (args) =>
                {
                    options.language = Localliztion.GetKey(args);
                    SetLanguage(options);
                });
            }

            GUILayout.EndHorizontal();
        }

        private void SetLanguage(UIBindData options)
        {
            string text = Localliztion.Get(options.language);
            if (text.EndsWith(".png") is false)
            {
                Text t = options.target.GetComponent<Text>();
                if (t != null)
                {
                    t.text = text;
                    return;
                }

                TMP_Text t2 = options.target.GetComponent<TMP_Text>();
                if (t2 != null)
                {
                    t2.text = text;
                    return;
                }

                return;
            }

            Image i = options.target.GetComponent<Image>();
            if (i != null)
            {
                if (text.StartsWith("Resources"))
                {
                    i.sprite = Resources.Load<Sprite>(text);
                }
                else
                {
                    i.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(text);
                }

                return;
            }

            RawImage i2 = options.target.GetComponent<RawImage>();
            if (i2 != null)
            {
                if (text.StartsWith("Resources"))
                {
                    i2.texture = Resources.Load<Texture>(text);
                }
                else
                {
                    i2.texture = AssetDatabase.LoadAssetAtPath<Texture>(text);
                }

                return;
            }
        }
    }
}
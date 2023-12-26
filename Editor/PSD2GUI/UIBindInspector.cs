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
using ZGame.Window;
using Object = UnityEngine.Object;

namespace ZGame.Editor.PSD2GUI
{
    [CustomEditor(typeof(UIBind))]
    public class UIBindInspector : CustomEditorWindow
    {
        private UIBind setting;


        public void OnEnable()
        {
            this.setting = (UIBind)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            setting.NameSpace = EditorGUILayout.TextField("NameSpace", setting.NameSpace);


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

                GUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                options.isOn = EditorGUILayout.Foldout(options.isOn, "");
                GUILayout.Space(-50);
                options.target = EditorGUILayout.ObjectField(options.target, typeof(Object), true) as GameObject;
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
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
            GetPath(options.target.transform, out string path);
            options.path = path;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Path", GUILayout.Width(110));
            GUILayout.Label(options.path);
            GUILayout.EndHorizontal();

            List<Component> opComs = options.target.GetComponents<Component>().ToList();
            options.selector.Add(opComs.Select(x => x.GetType().FullName).ToArray());

            GUILayout.BeginHorizontal();
            GUILayout.Label("Bind Components", GUILayout.Width(110));
            if (EditorGUILayout.DropdownButton(new GUIContent(options.selector.ToString()), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Noting"), options.selector.isNone, () => { options.selector.UnSelectAll(); });
                menu.AddItem(new GUIContent("Everything"), options.selector.isAll, () => { options.selector.SelectAll(); });
                foreach (var VARIABLE in options.selector.items)
                {
                    menu.AddItem(new GUIContent(VARIABLE.name), VARIABLE.isOn, () => { VARIABLE.isOn = !VARIABLE.isOn; });
                }

                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();


            if (opComs.Exists(x => x is Image) is false
                && opComs.Exists(x => x is Text) is false
                && opComs.Exists(x => x is TMP_Text) is false
                && opComs.Exists(x => x is RawImage) is false)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Is Bind Language", GUILayout.Width(110));
            options.bindLanguage = GUILayout.Toggle(options.bindLanguage, "");
            GUILayout.EndHorizontal();

            if (options.bindLanguage is false)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Bind Language", GUILayout.Width(110));
            LanguageDataList languageData = Language.instance._languages.Find(x => x.define == GlobalConfig.instance.language);
            if (languageData is not null && languageData.items is not null && languageData.items.Count > 0)
            {
                LanguageItem item = languageData.items.Find(x => x.key == options.language);
                if (item is not null)
                {
                    if (EditorGUILayout.DropdownButton(new GUIContent(item.value), FocusType.Passive))
                    {
                        List<string> selected = new List<string>() { item.value };
                        List<string> languages = languageData.items.Select(x => x.value).ToList();
                        ObjectSelectionWindow<string>.Show(new Vector2(200, 300), selected, languages, SelectionType.Single, () =>
                        {
                            int index = languageData.items.Find(x => x.value == selected[0]).key;
                            options.language = index;
                        });
                    }
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild
{
    [Options(typeof(RuleSeting))]
    [BindScene("规则管理", typeof(ResBuilder))]
    public class ResRuleSeting : PageScene
    {
        public override void OnEnable()
        {
            foreach (var ruler in BuilderConfig.instance.ruleSeting.rulers)
            {
                ruler.exs = new List<string>();
                if (ruler.folder == null)
                {
                    return;
                }

                string path = AssetDatabase.GetAssetPath(ruler.folder);
                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

                foreach (var VARIABLE in files)
                {
                    string ex = Path.GetExtension(VARIABLE);
                    if (ruler.exs.Contains(ex) || ex == ".meta")
                    {
                        continue;
                    }

                    ruler.exs.Add(ex);
                }
            }
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create"))
            {
                BuilderConfig.instance.ruleSeting.rulers.Add(new RulerInfoItem());
            }

            GUILayout.EndHorizontal();
            for (int i = BuilderConfig.instance.ruleSeting.rulers.Count - 1; i >= 0; i--)
            {
                if (search.IsNullOrEmpty() is false && BuilderConfig.instance.ruleSeting.rulers[i].name.Contains(search) is false)
                {
                    continue;
                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                DrawingRuleInfo(BuilderConfig.instance.ruleSeting.rulers[i]);
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            if (EditorGUI.EndChangeCheck())
            {
                BuilderConfig.Saved();
            }
        }

        private void DrawingRuleInfo(RulerInfoItem ruler)
        {
            GUILayout.BeginHorizontal();

            ruler.use = EditorGUILayout.Toggle("是否激活规则", ruler.use);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
            {
                BuilderConfig.instance.ruleSeting.rulers.Remove(ruler);
            }

            GUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(!ruler.use);
            GUILayout.Label(ruler.name);
            EditorGUI.BeginChangeCheck();
            ruler.folder = EditorGUILayout.ObjectField("资源目录", ruler.folder, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                OnEnable();
            }

            if (ruler.folder != null)
            {
                ruler.name = ruler.folder.name + " Ruler Seting";
            }

            GUILayout.BeginHorizontal();
            ruler.spiltPackageType = (SpiltPackageType)EditorGUILayout.EnumPopup("分包规则", ruler.spiltPackageType);
            if (ruler.folder != null && ruler.spiltPackageType == SpiltPackageType.AssetType)
            {
                string name = String.Empty;
                if (ruler.exList.Count == 0)
                {
                    name = "Noting";
                }
                else
                {
                    if (ruler.exs.Count == ruler.exList.Count)
                    {
                        name = "Everyting";
                    }
                    else
                    {
                        name = string.Join(",", ruler.exs);
                        if (name.Length > 20)
                        {
                            name = name.Substring(0, 25) + "...";
                        }
                    }
                }

                if (GUILayout.Button(name, EditorStyles.popup))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Noting"), ruler.exList.Count == 0, () => { ruler.exList.Clear(); });
                    menu.AddItem(new GUIContent("Everything"), ruler.exs.Count == ruler.exList.Count, () =>
                    {
                        ruler.exList.Clear();
                        ruler.exList.AddRange(ruler.exs);
                    });
                    foreach (var VARIABLE in ruler.exs)
                    {
                        menu.AddItem(new GUIContent(VARIABLE), ruler.exList.Contains(VARIABLE), () =>
                        {
                            if (ruler.exList.Contains(VARIABLE))
                            {
                                ruler.exList.Remove(VARIABLE);
                            }
                            else
                            {
                                ruler.exList.Add(VARIABLE);
                            }
                        });
                    }

                    menu.ShowAsContext();
                }
            }

            GUILayout.EndHorizontal();
            ruler.describe = EditorGUILayout.TextField("描述", ruler.describe);

            EditorGUI.EndDisabledGroup();
        }
    }

    class RulerGUI
    {
        public void OnGUI()
        {
        }
    }
}
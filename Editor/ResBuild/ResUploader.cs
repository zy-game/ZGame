using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    [BindScene("版本管理", typeof(ResBuilder))]
    public class ResUploader : PageScene
    {
        public OSSType type;
        private int selection = 0;
        private OSSManager manager;
        private string[] buckets;

        public override void OnEnable()
        {
            manager = new OSSManager();
            OnRefresh();
        }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal(ZStyle.GUI_STYLE_BOX_BACKGROUND);
            GUILayout.FlexibleSpace();
            type = (OSSType)EditorGUILayout.EnumPopup(type, EditorStyles.toolbarDropDown);
            EditorGUI.BeginChangeCheck();
            string[] list = BuilderConfig.instance.ossList.Where(x => x.type == type).Select(x => x.title).ToArray();
            selection = EditorGUILayout.Popup(selection, list, EditorStyles.toolbarDropDown);
            if (EditorGUI.EndChangeCheck())
            {
                OnRefresh();
            }

            if (GUILayout.Button("刷新", EditorStyles.toolbarButton))
            {
                OnRefresh();
            }

            if (GUILayout.Button("上传", EditorStyles.toolbarButton))
            {
                manager.Upload();
            }

            if (GUILayout.Button("下载", EditorStyles.toolbarButton))
            {
                manager.Download();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("File List", EditorStyles.helpBox);
            GUILayout.Space(15);
            if (manager is not null)
            {
                OnDrawingFolder(manager.root, 0);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void OnRefresh()
        {
            if (selection < 0 || selection > BuilderConfig.instance.ossList.Count - 1)
            {
                return;
            }

            manager.Refresh(BuilderConfig.instance.ossList.Where(x => x.type == type).ElementAt(selection));
        }


        private void OnDrawingFolder(OSSObject folder, int offset)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(offset);
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    folder.foldout = EditorGUILayout.Foldout(folder.foldout, "");
                    GUILayout.Space(-40);
                    GUILayout.Label(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(20), GUILayout.Height(20));
                    GUILayout.Label(folder.name);
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginChangeCheck();
                    folder.isOn = GUILayout.Toggle(folder.isOn, String.Empty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        folder.OnSelection(folder.isOn);
                    }

                    GUILayout.EndHorizontal();


                    if (folder.foldout)
                    {
                        if (folder.childs.Count > 0)
                        {
                            IEnumerable<OSSObject> folderList = folder.childs.Where(x => x.isFolder);
                            foreach (var VARIABLE in folderList)
                            {
                                OnDrawingFolder(VARIABLE, 15);
                            }

                            foreach (var VARIABLE in folder.childs)
                            {
                                if (VARIABLE.isFolder)
                                {
                                    continue;
                                }

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(offset);
                                GUILayout.Label(EditorGUIUtility.IconContent("Prefab Icon"), GUILayout.Width(20), GUILayout.Height(20));
                                GUILayout.Label(VARIABLE.name);
                                GUILayout.FlexibleSpace();
                                VARIABLE.isOn = GUILayout.Toggle(VARIABLE.isOn, String.Empty);
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }
}
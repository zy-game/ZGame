using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.Package
{
    [BindScene("包管理")]
    public class PackageManager : PageScene
    {
        private const string configPath = "Assets/Settings/ProjectPackageList.asset";
        private ProjectPackageDataList _projectPackageDataList;
        private string url;
        private string version;

        public override void OnEnable()
        {
            _projectPackageDataList = AssetDatabase.LoadAssetAtPath<ProjectPackageDataList>(configPath);
            if (_projectPackageDataList == null)
            {
                _projectPackageDataList = ScriptableObject.CreateInstance<ProjectPackageDataList>();
                AssetDatabase.CreateAsset(_projectPackageDataList, configPath);
            }

            EditorManager.instance.Waiting(); //ShowNotification(new GUIContent("Get Package List..."));
            _projectPackageDataList.Init(EditorManager.instance.CloseWaiting);
        }


        public override void OnDisable()
        {
            EditorManager.instance.RemoveNotification();
        }

        public override void OnGUI()
        {
            if (EditorApplication.isPlaying || _projectPackageDataList.isInit is false)
            {
                return;
            }

            GUILayout.BeginHorizontal(ZStyle.GUI_STYLE_BOX_BACKGROUND);
            url = EditorGUILayout.TextField("Package Name", url, EditorStyles.toolbarTextField, GUILayout.Width(400));
            version = EditorGUILayout.TextField("Version", version);
            if (GUILayout.Button("Add", EditorStyles.toolbarButton))
            {
                if (url.IsNullOrEmpty())
                {
                    EditorUtility.DisplayDialog("Error", "包名或包地址不能为空", "OK");
                }
                else
                {
                    _projectPackageDataList.OnUpdate(url, version);
                    EditorManager.Refresh();
                }
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Update All", EditorStyles.toolbarButton))
            {
                _projectPackageDataList.UpdateAll();
            }

            GUILayout.EndHorizontal();


            for (int i = 0; i < _projectPackageDataList.packages.Count; i++)
            {
                ProjectPackageData info = _projectPackageDataList.packages[i];
                if (info == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(search) is false && info.name.Contains(search) is false)
                {
                    continue;
                }

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(string.Format("{0}", info.name));
                if (info.state == PackageState.Update)
                {
                    GUILayout.Label(EditorGUIUtility.IconContent("AS Badge New"));
                }

                GUILayout.FlexibleSpace();
                // if (info.state == PackageState.Update)
                // {
                if (GUILayout.Button(info.version, EditorStyles.miniPullDown, GUILayout.Width(100)))
                {
                    GenericMenu menu = new GenericMenu();
                    for (int j = 0; j < info.versions.Count; j++)
                    {
                        string currentVersion = info.versions[j];
                        menu.AddItem(new GUIContent(currentVersion), currentVersion.EndsWith(info.version), () => { _projectPackageDataList.OnUpdate(info.name, currentVersion); });
                    }

                    menu.ShowAsContext();
                }

                GUILayout.Space(10);
                // }

                if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_MINUS))
                {
                    _projectPackageDataList.Remove(info);
                    EditorManager.Refresh();
                }


                GUILayout.EndHorizontal();
            }
        }
    }
}
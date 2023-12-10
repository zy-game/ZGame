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

        public override void OnEnable()
        {
            _projectPackageDataList = AssetDatabase.LoadAssetAtPath<ProjectPackageDataList>(configPath);
            if (_projectPackageDataList == null)
            {
                _projectPackageDataList = ScriptableObject.CreateInstance<ProjectPackageDataList>();
                AssetDatabase.CreateAsset(_projectPackageDataList, configPath);
            }

            EditorManager.instance.ShowNotification(new GUIContent("Get Package List..."));
            EditorManager.StartCoroutine(_projectPackageDataList.Init(OnDisable));
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

            if (GUILayout.Button("Add", EditorStyles.toolbarButton))
            {
                if (url.IsNullOrEmpty())
                {
                    EditorUtility.DisplayDialog("Error", "包名或包地址不能为空", "OK");
                }
                else
                {
                    ProjectPackageData packageData = new ProjectPackageData();
                    if (url.StartsWith("http"))
                    {
                        packageData.url = url;
                    }
                    else
                    {
                        packageData.name = name;
                    }

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
                GUILayout.Label(string.Format("{0}@{1} {3}", info.name, info.version, info.lastVersion, info.url));

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    _projectPackageDataList.Remove(info);
                    EditorManager.Refresh();
                }

                if (info.canUpdate)
                {
                    //todo 这里用下拉列表来做更新列表，这样可以更好的控制更新的版本
                    if (GUILayout.Button("Update", GUILayout.Width(70)))
                    {
                        _projectPackageDataList.UpdatePackage(info.name);
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}
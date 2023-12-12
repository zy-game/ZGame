using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.Package
{
    [BindScene("包管理")]
    public class PackageManager : PageScene
    {
        private const string configPath = "Assets/Settings/ProjectPackageList.asset";
        private string url;
        private string version;
        private int index = 0;
        private List<string> versionMap;

        public override void OnEnable()
        {
            EditorManager.instance.Waiting();
            PackageDataList.instance.ReferncePackageList(EditorManager.instance.CloseWaiting);
        }


        public override void OnDisable()
        {
            EditorManager.instance.RemoveNotification();
        }

        public override void OnGUI()
        {
            if (Application.isPlaying || PackageDataList.instance.isInit is false)
            {
                return;
            }

            OnShowPackageInstallHeader();

            OnShowNotInstallPackageList();
            OnShowPackageList();
        }

        private void OnShowPackageInstallHeader()
        {
            GUILayout.BeginHorizontal(ZStyle.GUI_STYLE_BOX_BACKGROUND);
            GUILayout.Label("Package Name");
            url = EditorGUILayout.TextField(url, GUILayout.Width(400));
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return && url.IsNullOrEmpty() is false)
            {
                index = 0;
                PackageDataList.instance.RequestVersionList(url, s => { versionMap = s; });
            }

            if (versionMap is not null && versionMap.Count > 0)
            {
                index = EditorGUILayout.Popup(index, versionMap.ToArray(), EditorStyles.toolbarDropDown);
                version = versionMap[index];
                if (GUILayout.Button("Install Package", EditorStyles.toolbarButton))
                {
                    if (url.IsNullOrEmpty())
                    {
                        EditorUtility.DisplayDialog("Error", "包名或包地址不能为空", "OK");
                    }
                    else
                    {
                        PackageDataList.instance.OnUpdate(url, version);
                        version = string.Empty;
                        versionMap = null;
                        index = 0;
                        url = string.Empty;
                        EditorManager.Refresh();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("Auto Installed");
            PackageDataList.instance.isAutoInstalled = EditorGUILayout.Toggle(PackageDataList.instance.isAutoInstalled, EditorStyles.toggle);
            if (GUILayout.Button("Update All", EditorStyles.toolbarButton))
            {
                foreach (var VARIABLE in PackageDataList.instance.packages)
                {
                    if (VARIABLE.versions.Equals(VARIABLE.recommended))
                    {
                        continue;
                    }

                    PackageDataList.instance.OnUpdate(VARIABLE.name, VARIABLE.recommended);
                }
            }

            GUILayout.EndHorizontal();
        }

        private void OnShowNotInstallPackageList()
        {
            for (int i = 0; i < PackageDataList.instance.remotePackages.Count; i++)
            {
                PackageData package = PackageDataList.instance.remotePackages[i];
                if (package == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(search) is false && package.title.Contains(search) is false)
                {
                    continue;
                }

                if (PackageDataList.instance.packages.Exists(x => x.name == package.name))
                {
                    continue;
                }

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(EditorGUIUtility.IconContent("d_winbtn_mac_max"));
                GUILayout.Label(string.Format("{0} [{1}]", package.title, package.recommended));
                GUILayout.FlexibleSpace();

                if (package.installed == InstallState.Uninstall)
                {
                    if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_ADD_BUTTON))
                    {
                        package.cur = 0;
                        EditorCoroutineUtility.StartCoroutineOwnerless(OnShow(package));
                        package.installed = InstallState.Installeding;
                        string version = package.recommended.IsNullOrEmpty() ? package.versions.Last() : package.recommended;
                        PackageDataList.instance.OnUpdate(package.name, version);
                        EditorManager.Refresh();
                    }
                }

                if (package.installed == InstallState.Installeding)
                {
                    GUILayout.Label(EditorGUIUtility.IconContent(package.icon));
                }

                GUILayout.EndHorizontal();
            }
        }


        private void OnShowPackageList()
        {
            for (int i = 0; i < PackageDataList.instance.packages.Count; i++)
            {
                PackageData package = PackageDataList.instance.packages[i];
                if (package == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(search) is false && package.title.Contains(search) is false)
                {
                    continue;
                }

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                if (package.installed == InstallState.None)
                {
                    GUILayout.Label(EditorGUIUtility.IconContent("d_winbtn_mac_close"));
                }
                else
                {
                    GUILayout.Label(EditorGUIUtility.IconContent(package.state switch
                    {
                        PackageState.None => "d_winbtn_mac_max",
                        PackageState.Update => "d_winbtn_mac_min",
                        _ => "d_winbtn_mac_max"
                    }));
                }

                GUILayout.Label(string.Format("{0} [{1}]", package.title, package.version));
                GUILayout.FlexibleSpace();

                if (package.state == PackageState.Update)
                {
                    if (GUILayout.Button(package.version, EditorStyles.miniPullDown, GUILayout.Width(100)))
                    {
                        GenericMenu menu = new GenericMenu();
                        for (int j = 0; j < package.versions.Count; j++)
                        {
                            string currentVersion = package.versions[j];
                            menu.AddItem(new GUIContent(currentVersion), currentVersion.EndsWith(package.version), () => { PackageDataList.instance.OnUpdate(package.name, currentVersion); });
                        }

                        menu.ShowAsContext();
                    }

                    GUILayout.Space(10);
                }

                if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_MINUS))
                {
                    PackageDataList.instance.Remove(package);
                    EditorManager.Refresh();
                }

                GUILayout.EndHorizontal();
            }
        }

        IEnumerator OnShow(PackageData info)
        {
            while (info.installed == InstallState.Installeding)
            {
                string temp = "WaitSpin";
                if (info.cur < 10)
                {
                    temp += "0" + info.cur;
                }
                else
                {
                    temp += info.cur;
                }

                info.icon = temp;
                EditorManager.Refresh();
                yield return new EditorWaitForSeconds(0.1f);
                info.cur++;
                info.cur %= info.end;
            }
        }
    }
}
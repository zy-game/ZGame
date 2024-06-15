using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.Resource;

namespace ZGame.Editor.Resource
{
    public class ResourcePackageScriptableObjectEditorWindow : ScriptableObjectEditorWindow<ResourcePackageListManifest>
    {
        private string[] packageList;
        private List<ResourceServerOptions> ossList;
        private Dictionary<ResourcePackageManifest, DefaultAsset> folderMap = new();
        public override Type owner => typeof(ResourceHomeEditorWindow);

        public ResourcePackageScriptableObjectEditorWindow(ResourcePackageListManifest data) : base(data)
        {
        }

        public override void OnEnable()
        {
            packageList = PackageManifestManager.GetAllManifestNames().Where(x => x != this.name).ToArray();
            if (target.packages is not null)
            {
                foreach (var VARIABLE in target.packages)
                {
                    if (folderMap.ContainsKey(VARIABLE))
                    {
                        continue;
                    }

                    DefaultAsset folder = null;
                    if (VARIABLE.foloder.IsNullOrEmpty() is false)
                    {
                        folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(VARIABLE.foloder);
                    }

                    folderMap.Add(VARIABLE, folder);
                }
            }

            ossList = AssetDatabase.FindAssets("t:ResourceServerOptions").Select(x => AssetDatabase.LoadAssetAtPath<ResourceServerOptions>(AssetDatabase.GUIDToAssetPath(x))).ToList();
        }

        private string GetdependenciesList()
        {
            if (this.target.dependencies is null || this.target.dependencies.Count == 0)
            {
                return "None";
            }

            if (this.target.dependencies.Count == packageList.Length)
            {
                return "Everything";
            }

            return string.Join(", ", this.target.dependencies);
        }

        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.PLAY_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                var menu = new GenericMenu();
                foreach (var VARIABLE in ossList)
                {
                    menu.AddItem(new GUIContent("Upload " + VARIABLE.name), false, () =>
                    {
                        using (BuildResourcePackageProceure proceure = new BuildResourcePackageProceure())
                        {
                            proceure.Execute(VARIABLE, target);
                        }
                    });
                }

                menu.ShowAsContext();
            }

            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.DELETE_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                this.OnDelete();
            }
        }

        public override void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
            GUILayout.Label("Dependencies", GUILayout.Width(145));
            if (EditorGUILayout.DropdownButton(new GUIContent(GetdependenciesList()), FocusType.Passive, EditorStyles.toolbarPopup))
            {
                SelectorWindow.ShowMulit(packageList.Select(x => new SelectorItem(x, x, target.dependencies.Contains(x))), x =>
                {
                    target.dependencies.Clear();
                    target.dependencies.AddRange(x.Select(y => y.userData.ToString()));
                });
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), target.dependencies.Count == 0, () => { target.dependencies.Clear(); });
                menu.AddItem(new GUIContent("Everything"), target.dependencies.Count == packageList.Length && target.dependencies.Count != 0, () =>
                {
                    target.dependencies.Clear();
                    target.dependencies.AddRange(packageList);
                });
                for (int i = 0; i < packageList.Length; i++)
                {
                    var packageName = packageList[i];
                    menu.AddItem(new GUIContent(packageName), target.dependencies.Contains(packageName), () =>
                    {
                        if (target.dependencies.Contains(packageName))
                        {
                            target.dependencies.Remove(packageName);
                        }
                        else
                        {
                            target.dependencies.Add(packageName);
                        }
                    });
                }

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(ZStyle.BOX_BACKGROUND);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Package List", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.ADD_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                ResourcePackageManifest manifest = new ResourcePackageManifest();
                this.target.packages.Add(manifest);
                folderMap.Add(manifest, null);
                EditorWindow.focusedWindow.Repaint();
            }

            GUILayout.EndHorizontal();
            if (target.packages is not null)
            {
                for (int i = target.packages.Count - 1; i >= 0; i--)
                {
                    var package = target.packages[i];
                    EditorGUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
                    folderMap[package] = EditorGUILayout.ObjectField(folderMap[package], typeof(DefaultAsset), false) as DefaultAsset;
                    if (folderMap[package] is not null)
                    {
                        package.foloder = AssetDatabase.GetAssetPath(folderMap[package]);
                    }

                    GUILayout.Label("Ruler", GUILayout.Width(50));
                    package.ruler = (BuildRuler)EditorGUILayout.EnumPopup(package.ruler, EditorStyles.toolbarPopup);
                    GUILayout.Label("Build Type", GUILayout.Width(80));
                    package.type = (BuildType)EditorGUILayout.EnumPopup(package.type, EditorStyles.toolbarPopup);
                    if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.DELETE_BUTTON_ICON), EditorStyles.toolbarButton))
                    {
                        this.target.packages.Remove(this.target.packages[i]);
                        EditorWindow.focusedWindow.Repaint();
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}
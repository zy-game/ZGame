using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Config;
using ZGame.Language;
using ZGame.Resource;

namespace ZGame.Editor
{
    public class SubGameScriptableObjectEditorWindow : ScriptableObjectEditorWindow<SubGameOptions>
    {
        private SceneAsset scene;
        private AssemblyDefinitionAsset dll;
        private List<ResourceServerOptions> ossList;
        private string[] packageList;
        public override Type owner => typeof(SubGameOptionHomeEditorWindow);

        public SubGameScriptableObjectEditorWindow(SubGameOptions data) : base(data)
        {
            if (data.scenePath.IsNullOrEmpty() is false)
            {
                scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(data.scenePath);
            }

            if (data.path.IsNullOrEmpty() is false)
            {
                dll = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(data.path);
            }
        }

        public override void OnEnable()
        {
            ossList = FindAllScriptableObject<ResourceServerOptions>();
            packageList = PackageManifestManager.GetAllManifestNames().ToArray();
        }

        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), GUIStyle.none))
            {
                SelectorWindow.Show(ossList.Select(x => new SelectorItem(x.name, x)), x =>
                {
                    if (target.mode == CodeMode.Hotfix)
                    {
                        using (BuildHotfixLibraryProceure proceure = new BuildHotfixLibraryProceure())
                        {
                            proceure.Execute(target);
                        }
                    }

                    using (BuildResourcePackageProceure resource = new BuildResourcePackageProceure())
                    {
                        resource.Execute(x, PackageManifestManager.GetPackageManifest(target.mainPackageName));
                    }

                    using (BuildGameChannelProceure proceure = new BuildGameChannelProceure())
                    {
                        proceure.Execute(target);
                    }
                });
            }

            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.DELETE_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                this.OnDelete();
            }
        }

        public override void OnGUI()
        {
            this.target.appName = EditorGUILayout.TextField("App Name", this.target.appName);
            this.target.args = EditorGUILayout.TextField("Args", this.target.args);
            this.target.version = EditorGUILayout.TextField("Version", this.target.version);
            this.target.packageName = EditorGUILayout.TextField("Install Package Name", this.target.packageName);
            this.target.language = (LanguageDefinition)EditorGUILayout.EnumPopup("Language", this.target.language);
            this.target.mode = (CodeMode)EditorGUILayout.EnumPopup("Code Mode", this.target.mode);
            int index = 0;
            index = EditorGUILayout.Popup("Package", index, packageList);
            if (index >= 0)
            {
                this.target.mainPackageName = packageList[index];
            }

            this.scene = EditorGUILayout.ObjectField("Scene", this.scene, typeof(SceneAsset), false) as SceneAsset;
            if (this.scene is not null)
            {
                this.target.scenePath = AssetDatabase.GetAssetPath(this.scene);
            }

            this.dll = EditorGUILayout.ObjectField("DLL", this.dll, typeof(AssemblyDefinitionAsset), false) as AssemblyDefinitionAsset;
            if (this.dll is not null)
            {
                this.target.path = AssetDatabase.GetAssetPath(this.dll);
            }

            this.target.icon = EditorGUILayout.ObjectField("Icon", this.target.icon, typeof(Texture2D), false) as Texture2D;
            this.target.splash = EditorGUILayout.ObjectField("Splash", this.target.splash, typeof(Sprite), false) as Sprite;
        }
    }
}
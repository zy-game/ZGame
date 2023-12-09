using System;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    [BindScene("设置", typeof(ResBuilder))]
    public class ResSetting : PageScene
    {
        public override void OnEnable()
        {
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("Output Seting", EditorStyles.helpBox);
            GUILayout.Space(20);
            BuilderConfig.instance.comperss = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("压缩方式", BuilderConfig.instance.comperss);
            BuilderConfig.instance.useActiveTarget = EditorGUILayout.Toggle("是否跟随激活平台", BuilderConfig.instance.useActiveTarget);
            EditorGUI.BeginDisabledGroup(BuilderConfig.instance.useActiveTarget);
            BuilderConfig.instance.target = (BuildTarget)EditorGUILayout.EnumPopup("编译平台", BuilderConfig.instance.target);
            EditorGUI.EndDisabledGroup();
            BuilderConfig.instance.fileExtension = EditorGUILayout.TextField("文件扩展名", BuilderConfig.instance.fileExtension);
            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.BeginVertical("OSS List", EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_ADD_BUTTON))
            {
                BuilderConfig.instance.ossList.Add(new OSSOptions());
                BuilderConfig.Saved();
                EditorManager.Refresh();
            }

            GUILayout.EndHorizontal();
            for (int i = 0; i < BuilderConfig.instance.ossList.Count; i++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_MINUS))
                {
                    BuilderConfig.instance.ossList.RemoveAt(i);
                    BuilderConfig.Saved();
                    EditorManager.Refresh();
                }

                GUILayout.EndHorizontal();
                DrawingOptionsItem(BuilderConfig.instance.ossList[i]);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();


            if (EditorGUI.EndChangeCheck())
            {
                BuilderConfig.Saved();
            }
        }

        // public string title;
        // public bool use;
        // public OSSType type;
        // public string appid;
        // public string bucket;
        // public string region;
        // public string address;
        // public string key;
        // public string password;
        private void DrawingOptionsItem(OSSOptions options)
        {
            options.title = EditorGUILayout.TextField("名称", options.title);
            options.type = (OSSType)EditorGUILayout.EnumPopup("类型", options.type);
            if (options.type == OSSType.Tencent)
            {
                options.appid = EditorGUILayout.TextField("AppId", options.appid);
            }

            options.region = EditorGUILayout.TextField("Region", options.region);

            options.bucket = EditorGUILayout.TextField("Bucket", options.bucket);
            // options.address = EditorGUILayout.TextField("Address", options.address);
            options.key = EditorGUILayout.TextField("SecretId", options.key);
            options.password = EditorGUILayout.TextField("SecretKey", options.password);
        }
    }
}
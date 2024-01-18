using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource.Config;

namespace ZGame.Editor.ResBuild
{
    [SubPageSetting("设置", typeof(ResBuilder))]
    public class ResSetting : SubPage
    {
        private bool outputIsOn = false;
        private bool resIsOn = false;

        public override void DrawingFoldoutHeaderRight(object userData)
        {
            if (userData is null)
            {
                return;
            }

            if (userData is List<OSSOptions> ossList)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ossList.Add(new OSSOptions());
                    BuilderConfig.OnSave();
                    OSSConfig.OnSave();
                    EditorManager.Refresh();
                }
            }

            if (userData is OSSOptions options)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    OSSConfig.instance.ossList.Remove(options);
                    BuilderConfig.OnSave();
                    OSSConfig.OnSave();
                    EditorManager.Refresh();
                }
            }
        }

        public override void OnGUI()
        {
            if (outputIsOn = OnShowFoldoutHeader("输出设置", outputIsOn, null))
            {
                GUILayout.BeginVertical("", ZStyle.BOX_BACKGROUND);
                GUILayout.Space(20);
                EditorGUI.BeginChangeCheck();
                BuilderConfig.instance.comperss = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("压缩方式", BuilderConfig.instance.comperss);
                BuilderConfig.instance.useActiveTarget = EditorGUILayout.Toggle("是否跟随激活平台", BuilderConfig.instance.useActiveTarget);
                EditorGUI.BeginDisabledGroup(BuilderConfig.instance.useActiveTarget);
                BuilderConfig.instance.target = (BuildTarget)EditorGUILayout.EnumPopup("编译平台", BuilderConfig.instance.target);
                EditorGUI.EndDisabledGroup();
                BuilderConfig.instance.fileExtension = EditorGUILayout.TextField("文件扩展名", BuilderConfig.instance.fileExtension);
                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    BuilderConfig.OnSave();
                    OSSConfig.OnSave();
                }
            }

            if (resIsOn = OnShowFoldoutHeader("资源服配置", resIsOn, OSSConfig.instance.ossList))
            {
                GUILayout.BeginVertical("", ZStyle.BOX_BACKGROUND);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
                for (int i = 0; i < OSSConfig.instance.ossList.Count; i++)
                {
                    OSSOptions options = OSSConfig.instance.ossList[i];
                    options.isOn = OnShowFoldoutHeader(options.title, options.isOn, options);
                    if (options.isOn is false)
                    {
                        continue;
                    }

                    GUILayout.BeginVertical(ZStyle.BOX_BACKGROUND);
                    DrawingOptionsItem(options);
                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();
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
            EditorGUI.BeginChangeCheck();
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
            if (EditorGUI.EndChangeCheck())
            {
                BuilderConfig.OnSave();
                OSSConfig.OnSave();
            }
        }
    }
}
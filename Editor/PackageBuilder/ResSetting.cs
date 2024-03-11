using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource.Config;

namespace ZGame.Editor.ResBuild
{
    [PageConfig("设置", typeof(ResBuilder), false, typeof(OSSConfig))]
    public class ResSetting : ToolbarScene
    {
        private bool outputIsOn = false;
        private bool resIsOn = false;

        public override void OnDrawingHeaderRight(object userData)
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
                    ToolsWindow.Refresh();
                }
            }

            if (userData is OSSOptions options)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    OSSConfig.instance.ossList.Remove(options);
                    BuilderConfig.OnSave();
                    OSSConfig.OnSave();
                    ToolsWindow.Refresh();
                }
            }
        }

        public override void OnGUI()
        {
            if (outputIsOn = OnBeginHeader("输出设置", outputIsOn, null))
            {
                GUILayout.BeginVertical("", EditorStyles.helpBox);
                GUILayout.Space(20);

                BuilderConfig.instance.comperss = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("压缩方式", BuilderConfig.instance.comperss);
                BuilderConfig.instance.useActiveTarget = EditorGUILayout.Toggle("是否跟随激活平台", BuilderConfig.instance.useActiveTarget);
                EditorGUI.BeginDisabledGroup(BuilderConfig.instance.useActiveTarget);
                BuilderConfig.instance.target = (BuildTarget)EditorGUILayout.EnumPopup("编译平台", BuilderConfig.instance.target);
                EditorGUI.EndDisabledGroup();
                BuilderConfig.instance.fileExtension = EditorGUILayout.TextField("文件扩展名", BuilderConfig.instance.fileExtension);
                GUILayout.EndVertical();
            }

            if (resIsOn = OnBeginHeader("资源服配置", resIsOn, OSSConfig.instance.ossList))
            {
                GUILayout.BeginVertical("", ZStyle.BOX_BACKGROUND);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
                for (int i = 0; i < OSSConfig.instance.ossList.Count; i++)
                {
                    OSSOptions options = OSSConfig.instance.ossList[i];
                    options.isOn = OnBeginHeader(options.title, options.isOn, options);
                    if (options.isOn)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        DrawingOptionsItem(options);
                        GUILayout.EndVertical();
                    }
                }

                GUILayout.EndVertical();
            }

            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                BuilderConfig.OnSave();
                OSSConfig.OnSave();
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
            if (options.type == OSSType.Streaming)
            {
                if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
                {
                    BuilderConfig.OnSave();
                    OSSConfig.OnSave();
                }

                return;
            }

            options.region = EditorGUILayout.TextField("Region", options.region);
            options.bucket = EditorGUILayout.TextField("Bucket", options.bucket);
            options.key = EditorGUILayout.TextField("SecretId", options.key);
            options.password = EditorGUILayout.TextField("SecretKey", options.password);
            options.enableAccelerate = EditorGUILayout.Toggle("启用全球加速", options.enableAccelerate);
            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                BuilderConfig.OnSave();
                OSSConfig.OnSave();
            }
        }
    }
}
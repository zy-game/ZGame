// using System;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
// using ZGame.Editor.ResBuild.Config;
// using ZGame.VFS;
//
// namespace ZGame.Editor.ResBuild
// {
//     [GameSubEditorWindowOptions("设置", typeof(ResBuilder), false, typeof(ResConfig))]
//     public class ResSetting : GameSubEditorWindow
//     {
//         private bool outputIsOn = false;
//         private bool resIsOn = false;
//         private bool vfsIsOn = false;
//
//         public override void OnDrawingHeaderRight(object userData)
//         {
//             if (userData is null)
//             {
//                 return;
//             }
//
//             if (userData is List<OSSOptions> ossList)
//             {
//                 if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
//                 {
//                     ResConfig.instance.Add(new OSSOptions());
//                     ResConfig.Save();
//                     GameBaseEditorWindow.Refresh();
//                 }
//             }
//
//             if (userData is OSSOptions options)
//             {
//                 if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
//                 {
//                     ResConfig.instance.Remove(options);
//                     ResConfig.Save();
//
//                     GameBaseEditorWindow.Refresh();
//                 }
//             }
//         }
//
//         public override void OnGUI()
//         {
//             if (outputIsOn = OnBeginHeader("输出设置", outputIsOn, null))
//             {
//                 GUILayout.BeginVertical("", EditorStyles.helpBox);
//                 GUILayout.Space(20);
//
//                 BuilderConfig.instance.comperss = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("压缩方式", BuilderConfig.instance.comperss);
//                 BuilderConfig.instance.useActiveTarget = EditorGUILayout.Toggle("是否跟随激活平台", BuilderConfig.instance.useActiveTarget);
//                 EditorGUI.BeginDisabledGroup(BuilderConfig.instance.useActiveTarget);
//                 BuilderConfig.instance.target = (BuildTarget)EditorGUILayout.EnumPopup("编译平台", BuilderConfig.instance.target);
//                 EditorGUI.EndDisabledGroup();
//                 BuilderConfig.instance.fileExtension = EditorGUILayout.TextField("文件扩展名", BuilderConfig.instance.fileExtension);
//                 GUILayout.EndVertical();
//             }
//
//             if (resIsOn = OnBeginHeader("资源服配置", resIsOn, ResConfig.instance.ossList))
//             {
//                 GUILayout.BeginVertical("", ZStyle.BOX_BACKGROUND);
//                 GUILayout.BeginHorizontal();
//                 GUILayout.FlexibleSpace();
//
//                 GUILayout.EndHorizontal();
//                 for (int i = 0; i < ResConfig.instance.ossList.Count; i++)
//                 {
//                     OSSOptions options = ResConfig.instance.ossList[i];
//                     // options.isOn = OnBeginHeader(options.title, options.isOn, options);
//                     // if (options.isOn)
//                     // {
//                     //     GUILayout.BeginVertical(EditorStyles.helpBox);
//                     //     DrawingOptionsItem(options);
//                     //     GUILayout.EndVertical();
//                     // }
//                 }
//
//                 GUILayout.EndVertical();
//             }
//
//             if (vfsIsOn = OnBeginHeader("虚拟文件系统设置",vfsIsOn))
//             {
//                 ResConfig.instance.enable = EditorGUILayout.Toggle("是否开启虚拟文件系统", ResConfig.instance.enable);
//                 ResConfig.instance.chunkSize = EditorGUILayout.IntField("分片大小", ResConfig.instance.chunkSize);
//                 ResConfig.instance.chunkCount = EditorGUILayout.IntField("单个文件最大分片数", ResConfig.instance.chunkCount);
//             }
//         }
//
//         public override void SaveChanges()
//         {
//             BuilderConfig.Save();
//             ResConfig.Save();
//         }
//
//         // public string title;
//         // public bool use;
//         // public OSSType type;
//         // public string appid;
//         // public string bucket;
//         // public string region;
//         // public string address;
//         // public string key;
//         // public string password;
//         private void DrawingOptionsItem(OSSOptions options)
//         {
//             options.title = EditorGUILayout.TextField("名称", options.title);
//             options.type = (OSSType)EditorGUILayout.EnumPopup("类型", options.type);
//             options.region = EditorGUILayout.TextField("Region", options.region);
//             options.bucket = EditorGUILayout.TextField("Bucket", options.bucket);
//             options.key = EditorGUILayout.TextField("SecretId", options.key);
//             options.password = EditorGUILayout.TextField("SecretKey", options.password);
//             options.enableAccelerate = EditorGUILayout.Toggle("启用全球加速", options.enableAccelerate);
//         }
//     }
// }
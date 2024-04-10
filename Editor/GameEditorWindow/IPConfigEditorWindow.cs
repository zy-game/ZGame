// using UnityEditor;
// using UnityEngine;
//
// namespace ZGame.Editor
// {
//     [GameSubEditorWindowOptions("服务器地址", typeof(RuntimeEditorWindow))]
//     public class IPConfigEditorWindow : GameSubEditorWindow
//     {
//         public override void OnGUI()
//         {
//             GUILayout.BeginVertical(EditorStyles.helpBox);
//             GUILayout.BeginHorizontal();
//             GUILayout.Label("Address", EditorStyles.boldLabel);
//             GUILayout.FlexibleSpace();
//             if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
//             {
//                 IPConfig.instance.Add(new IPOptions());
//             }
//
//             GUILayout.EndHorizontal();
//             for (int i = 0; i < IPConfig.instance.ipList.Count; i++)
//             {
//                 IPOptions config = IPConfig.instance.ipList[i];
//                 config.isOn = OnBeginHeader(config.title, config.isOn, config);
//                 if (config.isOn)
//                 {
//                     config.title = EditorGUILayout.TextField("别名", config.title);
//                     config.address = EditorGUILayout.TextField("IP", config.address);
//                     config.port = EditorGUILayout.IntField("端口", config.port);
//                 }
//             }
//
//             GUILayout.EndVertical();
//         }
//
//         public override void SaveChanges()
//         {
//             IPConfig.Save();
//         }
//     }
// }
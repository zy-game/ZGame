using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    [PageConfig("Address", typeof(RuntimeEditorWindow))]
    public class AddressWindow : ToolbarScene
    {
        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.address.Add(new IPConfig());
            }
        }

        public override void OnDrawingHeaderRight(object userData)
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.address.Remove((IPConfig)userData);
            }
        }

        public override void OnGUI()
        {
            for (int i = 0; i < BasicConfig.instance.address.Count; i++)
            {
                IPConfig config = BasicConfig.instance.address[i];
                config.isOn = OnBeginHeader(config.title, config.isOn, config);
                if (config.isOn)
                {
                    EditorGUI.BeginChangeCheck();
                    OnShowAddressConfig(config);
                    if (EditorGUI.EndChangeCheck())
                    {
                        BasicConfig.OnSave();
                    }
                }
            }
        }

        private void OnShowAddressConfig(IPConfig config)
        {
            config.title = EditorGUILayout.TextField("别名", config.title);
            config.address = EditorGUILayout.TextField("IP", config.address);
            config.port = EditorGUILayout.IntField("端口", config.port);
        }
    }
}
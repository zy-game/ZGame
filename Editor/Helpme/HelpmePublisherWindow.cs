using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.Helpme
{
    [SubPageSetting("编辑", typeof(HelpmeWindow))]
    public class HelpmePublisherWindow : SubPage
    {
        private HelpData helpmeData;


        public override void OnEnable(params object[] args)
        {
            helpmeData = new HelpData();
        }

        public override void SearchRightDrawing()
        {
            if (GUILayout.Button("发布", ZStyle.HEADER_BUTTON_STYLE))
            {
                helpmeData = new HelpData();
            }
        }

        public override void OnGUI()
        {
            if (helpmeData is null)
            {
                return;
            }

            helpmeData.title = EditorGUILayout.TextField("标题", helpmeData.title);
            GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
            if (GUILayout.Button("保存", ZStyle.HEADER_BUTTON_STYLE))
            {
            }

            if (GUILayout.Button("插入图片", ZStyle.HEADER_BUTTON_STYLE))
            {
            }

            if (GUILayout.Button("插入链接", ZStyle.HEADER_BUTTON_STYLE))
            {
            }

            if (GUILayout.Button("插入代码", ZStyle.HEADER_BUTTON_STYLE))
            {
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            helpmeData.content = EditorGUILayout.TextArea(helpmeData.content, GUILayout.Height(position.height - 70), GUILayout.Width(position.width));
        }
    }
}
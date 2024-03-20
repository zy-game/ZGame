using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.Helpme
{
    class ChatData
    {
        public string user;
        public string content;
        public string time;
        public List<string> like;
        public List<string> unlike;
        public List<ChatData> chats;
    }

    class HelpData
    {
        public string title;
        public string content;
        public string user;
        public string time;
        public uint like;
        public uint unlike;
        public bool isTop;
        public List<ChatData> chats;
    }

    [GameSubEditorWindowOptions("Think tanks")]
    public class ThinktanksWindow : GameSubEditorWindow
    {
        private List<HelpData> helps;

        public override void OnEnable(params object[] args)
        {
            //todo 从服务器取数据
            helps = new List<HelpData>();
            for (int i = 0; i < 10; i++)
            {
                helps.Add(new HelpData
                {
                    title = "1.如何使用",
                    content = "这里是详细文本",
                    user = "zhangsan",
                    time = "2020-01-01 12:00:00",
                    like = 10,
                    unlike = 10,
                    isTop = true,
                    chats = new List<ChatData>
                    {
                        new ChatData()
                        {
                            user = "张山",
                            content = "你这个太几把吊了",
                            time = "2020-01-01 12:00:00",
                            like = new List<string>(),
                            unlike = new List<string>()
                        }
                    }
                });
            }
        }

        public override void OnGUI()
        {
            foreach (var VARIABLE in helps)
            {
                if (search.IsNullOrEmpty() is false && (VARIABLE.title.Contains(search) || VARIABLE.content.Contains(search)) is false)
                {
                    continue;
                }

                Rect rect = EditorGUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                EditorGUILayout.LabelField(VARIABLE.title, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label(VARIABLE.user);
                GUILayout.Space(20);
                GUILayout.Label(VARIABLE.time);
                if (this.OnMouseLeftButtonDown(rect))
                {
                    GameBaseEditorWindow.SwitchScene<ReaderWindow>(VARIABLE);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Markdig;
using Markdig.Syntax;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.Helpme
{
    [GameSubEditorWindowOptions("详细", typeof(ThinktanksWindow), true)]
    public class ReaderWindow : GameSubEditorWindow
    {
        private HelpData helpmeData;
        private MarkdownDocument document;
        private ChatData comment;
        private string commentContent;

        public override string name
        {
            get
            {
                if (helpmeData is null)
                {
                    return base.name;
                }

                return helpmeData.title;
            }
        }

        public override void OnEnable(params object[] args)
        {
            if (args is null || args.Length is 0)
            {
                return;
            }

            helpmeData = args[0] as HelpData;
            document = Markdown.Parse(helpmeData.content);
        }

        public override void OnGUI()
        {
            if (helpmeData is null || document is null)
            {
                return;
            }

            OnDrawingHelpInfo();
            OnDrawingChetList();
        }

        private void OnDrawingHelpInfo()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(helpmeData.content);
            GUILayout.EndVertical();
        }

        private void OnDrawingChetList()
        {
            if (helpmeData.chats is null || helpmeData.chats.Count is 0)
            {
                return;
            }

            foreach (var VARIABLE in helpmeData.chats)
            {
                OnDrawingChatInfo(VARIABLE, position.width);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            commentContent = GUILayout.TextField(commentContent, GUILayout.Width(position.width - 100));
            if (GUILayout.Button("发表", GUILayout.Width(50)))
            {
                ChatData chatData = new ChatData()
                {
                    user = ThinktanksConfig.instance.userName,
                    like = new List<string>(),
                    unlike = new List<string>(),
                    time = DateTime.Now.ToString("s"),
                    content = commentContent
                };
                if (comment is null)
                {
                    helpmeData.chats.Add(chatData);
                }
                else
                {
                    if (comment.chats is null)
                    {
                        comment.chats = new List<ChatData>();
                    }

                    comment.chats.Add(chatData);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnDrawingChatInfo(ChatData VARIABLE, float width)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("SoftlockInline"), GUILayout.Width(20));
            EditorGUILayout.LabelField(VARIABLE.user, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label(VARIABLE.time);
            GUILayout.EndHorizontal();
            OnDrawingSplitLine(width, Color.black);
            GUILayout.Space(5);
            GUILayout.Label(VARIABLE.content, GUILayout.MaxWidth(width));
            if (VARIABLE.chats is not null && VARIABLE.chats.Count > 0)
            {
                foreach (var chat in VARIABLE.chats)
                {
                    OnDrawingChatInfo(chat, width - 20);
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent("Packages/com.zh.zgame/Editor/EditorResources/chat.png"), ZStyle.HEADER_BUTTON_STYLE, GUILayout.Width(20), GUILayout.Height(20)))
            {
                comment = VARIABLE;
            }

            GUILayout.Label(VARIABLE.like.Count.ToString());
            if (GUILayout.Button(EditorGUIUtility.IconContent("Packages/com.zh.zgame/Editor/EditorResources/like.png"), ZStyle.HEADER_BUTTON_STYLE, GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (VARIABLE.like.Contains(ThinktanksConfig.instance.userName))
                {
                    VARIABLE.like.Remove(ThinktanksConfig.instance.userName);
                }
                else
                {
                    VARIABLE.like.Add(ThinktanksConfig.instance.userName);
                }

                VARIABLE.unlike.Remove(ThinktanksConfig.instance.userName);
            }

            GUILayout.Label(VARIABLE.unlike.Count.ToString());
            if (GUILayout.Button(EditorGUIUtility.IconContent("Packages/com.zh.zgame/Editor/EditorResources/unlike.png"), ZStyle.HEADER_BUTTON_STYLE, GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (VARIABLE.unlike.Contains(ThinktanksConfig.instance.userName))
                {
                    VARIABLE.unlike.Remove(ThinktanksConfig.instance.userName);
                }
                else
                {
                    VARIABLE.unlike.Add(ThinktanksConfig.instance.userName);
                }

                VARIABLE.like.Remove(ThinktanksConfig.instance.userName);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
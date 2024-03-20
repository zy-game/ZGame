using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    [GameSubEditorWindowOptions("渠道包设置", typeof(RuntimeEditorWindow))]
    public class ChannelPackageEditorWindow : GameSubEditorWindow
    {
        public override void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Channels", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                ChannelConfigList.instance.Add(new ChannelPackageOptions());
                ChannelConfigList.Save();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndVertical();
            for (int j = ChannelConfigList.instance.pckList.Count - 1; j >= 0; j--)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                // BasicConfig.instance.curEntry.channels[j].title = EditorGUILayout.TextField("Channel Name", BasicConfig.instance.curEntry.channels[j].title);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ChannelConfigList.instance.Remove(ChannelConfigList.instance.pckList[j]);
                    ChannelConfigList.Save();
                }

                GUILayout.EndHorizontal();
                ChannelConfigList.instance.pckList[j].title = EditorGUILayout.TextField("渠道名", ChannelConfigList.instance.pckList[j].title);

                List<string> title = LanguageConfig.instance.lanList.Select(x => x.name).ToList();
                List<string> filters = LanguageConfig.instance.lanList.Select(x => x.filter).ToList();
                int last = filters.FindIndex(x => x == ChannelConfigList.instance.pckList[j].language);
                int curIndex = EditorGUILayout.Popup("语言", last, title.ToArray());
                if (curIndex != last)
                {
                    ChannelConfigList.instance.pckList[j].language = LanguageConfig.instance.lanList[curIndex].filter;
                }

                ChannelConfigList.instance.pckList[j].args = EditorGUILayout.TextField("Args", ChannelConfigList.instance.pckList[j].args);
                ChannelConfigList.instance.pckList[j].packageName = EditorGUILayout.TextField("Package Name", ChannelConfigList.instance.pckList[j].packageName);
                ChannelConfigList.instance.pckList[j].appName = EditorGUILayout.TextField("App Name", ChannelConfigList.instance.pckList[j].appName);
                ChannelConfigList.instance.pckList[j].icon = (EditorGUILayout.ObjectField("Channel Icon", ChannelConfigList.instance.pckList[j].icon, typeof(Texture2D), false) as Texture2D);
                ChannelConfigList.instance.pckList[j].splash = (EditorGUILayout.ObjectField("Channel Splash", ChannelConfigList.instance.pckList[j].splash, typeof(Sprite), false) as Sprite);
                GUILayout.EndVertical();
            }
        }

        public override void SaveChanges()
        {
            ChannelConfigList.Save();
        }
    }
}
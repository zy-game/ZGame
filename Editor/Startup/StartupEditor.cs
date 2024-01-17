using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZGame.Resource.Config;

namespace ZGame.Editor
{
    [CustomEditor(typeof(Startup))]
    public class StartupEditor : CustomEditorWindow
    {
        private Startup startup;

        private void OnEnable()
        {
            startup = (Startup)target;
        }

        public override void OnInspectorGUI()
        {
            int last = 0;
            int curIndex = 0;
            EditorGUI.BeginChangeCheck();
            last = BasicConfig.instance.entries.FindIndex(x => x.title == BasicConfig.instance.curEntryName);
            curIndex = EditorGUILayout.Popup("模块入口", last, BasicConfig.instance.entries.Select(x => x.title).ToArray());

            if (curIndex >= 0 && curIndex < BasicConfig.instance.entries.Count && last != curIndex)
            {
                BasicConfig.instance.curEntryName = BasicConfig.instance.entries[curIndex].title;
            }

            GUILayout.BeginHorizontal();
            last = BasicConfig.instance.address.FindIndex(x => x.title == BasicConfig.instance.curAddressName);
            curIndex = EditorGUILayout.Popup("服务器地址", last, BasicConfig.instance.address.Select(x => x.title).ToArray());
            GUILayout.EndHorizontal();
            if (curIndex >= 0 && curIndex < BasicConfig.instance.address.Count && last != curIndex)
            {
                BasicConfig.instance.curAddressName = BasicConfig.instance.address[curIndex].title;
            }

            BasicConfig.instance.resMode = (ResourceMode)EditorGUILayout.EnumPopup("资源模式", BasicConfig.instance.resMode);

            if (BasicConfig.instance.resMode == ResourceMode.Simulator)
            {
                BasicConfig.instance.resTimeout = EditorGUILayout.Slider("包检查间隔时间", BasicConfig.instance.resTimeout, 10, byte.MaxValue);
                last = OSSConfig.instance.ossList.FindIndex(x => x.title == OSSConfig.instance.seletion);
                curIndex = EditorGUILayout.Popup("资源服务器地址", last, OSSConfig.instance.ossList.Select(x => x.title).ToArray());

                if (curIndex >= 0 && curIndex < OSSConfig.instance.ossList.Count && last != curIndex)
                {
                    OSSConfig.instance.seletion = OSSConfig.instance.ossList[curIndex].title;
                    OSSConfig.OnSave();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                OSSConfig.OnSave();
                BasicConfig.OnSave();
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using ZGame.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.Inspector
{
    [CustomEditor(typeof(AppStart))]
    public class AppStartupInspector : UnityEditor.Editor
    {
        private AppStart _start;
        private List<SubGameOptions> gameList;
        private List<GameServerOptions> serverList;
        private List<ResourceServerOptions> ossList;

        void OnEnable()
        {
            _start = target as AppStart;
            gameList = FindAllScriptableObject<SubGameOptions>();
            serverList = FindAllScriptableObject<GameServerOptions>();
            ossList = FindAllScriptableObject<ResourceServerOptions>();
        }

        private List<T> FindAllScriptableObject<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T)}").Select(x => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(x))).ToList();
        }

        IEnumerable<SelectorItem> GetResModeItemList()
        {
            return new List<SelectorItem>()
            {
                new SelectorItem(null, ResourceMode.Editor.ToString(), ResourceMode.Editor),
                new SelectorItem(null, ResourceMode.Simulator.ToString(), ResourceMode.Simulator),
            };
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            GUILayout.BeginHorizontal();
            GUILayout.Label("日志输出", EditorStyles.boldLabel, GUILayout.Width(150));
            _start.isDebug = EditorGUILayout.Toggle("", _start.isDebug);
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label("资源模式", EditorStyles.boldLabel, GUILayout.Width(150));
            if (EditorGUILayout.DropdownButton(new GUIContent(_start.resMode.ToString()), FocusType.Passive))
            {
                SelectorWindow.Show(GetResModeItemList(), x => _start.resMode = (ResourceMode)x.FirstOrDefault().userData);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("是否全屏", EditorStyles.boldLabel, GUILayout.Width(150));
            _start.isFullScreen = EditorGUILayout.Toggle("", _start.isFullScreen);
            GUILayout.EndHorizontal();
            if (_start.isFullScreen is false)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("分辨率", EditorStyles.boldLabel, GUILayout.Width(150));
                _start.resolution = EditorGUILayout.Vector2Field("", _start.resolution);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("资源服务器设置", EditorStyles.boldLabel, GUILayout.Width(150));
            if (EditorGUILayout.DropdownButton(new GUIContent(_start.ossOptions == null ? "None" : _start.ossOptions.name), FocusType.Passive))
            {
                SelectorWindow.Show(ossList.Select(x => new SelectorItem(x.name, x)),
                    x => _start.ossOptions = (ResourceServerOptions)x.FirstOrDefault().userData);
            }


            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("游戏服务器设置", EditorStyles.boldLabel, GUILayout.Width(150));
            if (EditorGUILayout.DropdownButton(new GUIContent(_start.gameServerOptions == null ? "None" : _start.gameServerOptions.name), FocusType.Passive))
            {
                SelectorWindow.Show(serverList.Select(x => new SelectorItem(x.name, x)),
                    x => _start.gameServerOptions = (GameServerOptions)x.FirstOrDefault().userData);
            }


            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("游戏入口设置", EditorStyles.boldLabel, GUILayout.Width(150));
            if (EditorGUILayout.DropdownButton(new GUIContent(_start.subGame == null ? "None" : _start.subGame.name), FocusType.Passive))
            {
                SelectorWindow.Show(gameList.Select(x => new SelectorItem(x.name, x)),
                    x => _start.subGame = (SubGameOptions)x.FirstOrDefault().userData);
            }


            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(_start);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public partial class EditorManager
    {
        private static EditorManager _docker;
        private static List<SubPage> sceneMaps;
        private static UnityEngine.Object openScriptableObject;
        private EditorCoroutine waiting;
        private bool isWaiting;


        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptBuilderComplation()
        {
            sceneMaps = new List<SubPage>();
            List<Type> types = AppDomain.CurrentDomain.GetAllSubClasses<SubPage>();


            foreach (var VARIABLE in types)
            {
                SubPageSetting setting = VARIABLE.GetCustomAttribute<SubPageSetting>();

                if (setting is null || setting.parent is not null)
                {
                    continue;
                }

                sceneMaps.Add((SubPage)Activator.CreateInstance(VARIABLE));
            }

            foreach (var VARIABLE in types)
            {
                SubPageSetting setting = VARIABLE.GetCustomAttribute<SubPageSetting>();
                if (setting is null || setting.parent is null)
                {
                    continue;
                }

                foreach (var VARIABLE2 in sceneMaps)
                {
                    if (VARIABLE2.GetType().Equals(setting.parent))
                    {
                        VARIABLE2.childs.Add((SubPage)Activator.CreateInstance(VARIABLE));
                        break;
                    }
                }
            }

            if (EditorWindow.HasOpenInstances<EditorManager>() is false)
            {
                return;
            }

            OpenScene();
        }

        private static EditorManager TryOpenManager()
        {
            return _docker = EditorManager.GetWindow<EditorManager>(false, "工具集", true);
        }


        [MenuItem("Tools/ZGame Editor %L")]
        static void OpenScene()
        {
            TryOpenManager();
            SwitchScene(sceneMaps.FirstOrDefault());
        }


        public static SubPage GetScene(Type type)
        {
            if (type.IsSubclassOf(typeof(SubPage)) is false)
            {
                return default;
            }

            foreach (var VARIABLE in sceneMaps)
            {
                if (VARIABLE.GetType().Equals(type))
                {
                    return VARIABLE;
                }

                SubPage page = VARIABLE.childs.Find(x => x.GetType().Equals(type));
                if (page is null)
                {
                    continue;
                }

                return page;
            }

            return default;
        }

        public static T GetScene<T>() where T : SubPage
        {
            return (T)GetScene(typeof(T));
        }

        public static void SwitchScene<T>(params object[] args) where T : SubPage
        {
            SwitchScene(typeof(T), args);
        }

        public static void SwitchScene(Type type, params object[] args)
        {
            //检查type是否实现了PageScene
            if (type.IsSubclassOf(typeof(SubPage)) is false)
            {
                return;
            }

            SwitchScene(GetScene(type), args);
        }

        public static void SwitchScene(SubPage scene, params object[] args)
        {
            if (scene is null)
            {
                return;
            }

            if (_docker == null)
            {
                TryOpenManager();
            }

            if (_docker.current is not null)
            {
                _docker.CloseWaiting();
                _docker.current.StopAllCoroutine();
                _docker.current.OnDisable();
            }

            _docker.current = scene;
            _docker.current.OnEnable(args);
            Refresh();
        }

        public static EditorCoroutine StartCoroutine(IEnumerator enumerator)
        {
            if (_docker is null)
            {
                return default;
            }

            return _docker.current.StartCoroutine(enumerator);
        }

        public static void ShowWait()
        {
            _docker.Waiting();
        }

        public static void CloseWait()
        {
            _docker.CloseWaiting();
        }

        public static void Refresh()
        {
            _docker?.Repaint();
        }
    }
}
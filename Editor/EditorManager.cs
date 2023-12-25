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
        private static List<SceneData> sceneMaps;
        private static UnityEngine.Object openScriptableObject;
        private EditorCoroutine waiting;
        private bool isWaiting;

        class SceneData
        {
            public bool show;
            public SubPage scene;
            public List<SubPage> childs;
            public Type settingType;
            public Type parent;
        }

        public static EditorManager instance
        {
            get { return _docker; }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptBuilderComplation()
        {
            sceneMaps = new List<SceneData>();
            List<Type> types = AppDomain.CurrentDomain.GetAllSubClasses<SubPage>();


            foreach (var VARIABLE in types)
            {
                SubPageSetting editorScene = VARIABLE.GetCustomAttribute<SubPageSetting>();

                if (editorScene is null)
                {
                    continue;
                }

                SceneData sceneData = new SceneData();
                sceneMaps.Add(sceneData);
                sceneData.scene = (SubPage)Activator.CreateInstance(VARIABLE);
                sceneData.parent = editorScene.parent;
                sceneData.childs = new List<SubPage>();
                ReferenceScriptableObject referenceScriptableObject = VARIABLE.GetCustomAttribute<ReferenceScriptableObject>();
                if (referenceScriptableObject is null)
                {
                    continue;
                }

                sceneData.settingType = referenceScriptableObject.type;
            }

            for (int i = sceneMaps.Count - 1; i >= 0; i--)
            {
                SceneData sceneData = sceneMaps[i];
                if (sceneData.parent is null)
                {
                    continue;
                }

                SceneData parent = sceneMaps.Find(x => x.scene.GetType() == sceneData.parent);
                if (parent is null)
                {
                    continue;
                }

                parent.childs.Add(sceneData.scene);
                sceneMaps.RemoveAt(i);
            }


            if (EditorWindow.HasOpenInstances<EditorManager>() is false)
            {
                return;
            }

            OpenScene();
        }

        [MenuItem("Tools/ZGame Editor %L")]
        static void OpenScene()
        {
            OpenScene(null);
        }

        [OnOpenAsset()]
        static bool OnOpened(int id, int line)
        {
            UnityEngine.Object target = UnityEditor.EditorUtility.InstanceIDToObject(id);
            if (target == null)
            {
                return false;
            }

            if (sceneMaps.Exists(x => x.settingType == target.GetType()) is false)
            {
                return false;
            }

            OpenScene(target);
            return true;
        }

        static void OpenScene(Object obj)
        {
            if (_docker == null)
            {
                _docker = GetWindow<EditorManager>(false, "编辑器", true);
                if (_docker == null)
                {
                    return;
                }
            }

            if (obj == null)
            {
                SwitchScene(sceneMaps.FirstOrDefault()?.scene);
                return;
            }

            SubPage subPage = sceneMaps.Find(x => x.settingType == obj.GetType())?.scene;
            if (subPage is null)
            {
                return;
            }

            SwitchScene(subPage);
        }

        public static SubPage GetScene(Type type)
        {
            if (type.IsSubclassOf(typeof(SubPage)) is false)
            {
                return default;
            }

            foreach (var VARIABLE in sceneMaps)
            {
                if (VARIABLE.scene.GetType().Equals(type))
                {
                    return VARIABLE.scene;
                }

                if (VARIABLE.childs.Exists(x => x.GetType().Equals(type)))
                {
                    return VARIABLE.childs.Find(x => x.GetType().Equals(type));
                }
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
            return instance.current.StartCoroutine(enumerator);
        }

        public static void Refresh()
        {
            _docker?.Repaint();
        }
    }
}
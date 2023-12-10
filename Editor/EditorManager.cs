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

        class SceneData
        {
            public bool show;
            public PageScene scene;
            public List<PageScene> childs;
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
            List<Type> types = AppDomain.CurrentDomain.GetAllSubClasses<PageScene>();


            foreach (var VARIABLE in types)
            {
                BindScene editorScene = VARIABLE.GetCustomAttribute<BindScene>();

                if (editorScene is null)
                {
                    continue;
                }

                SceneData sceneData = new SceneData();
                sceneMaps.Add(sceneData);
                sceneData.scene = (PageScene)Activator.CreateInstance(VARIABLE);
                sceneData.parent = editorScene.parent;
                sceneData.childs = new List<PageScene>();
                SettingContent settingContent = VARIABLE.GetCustomAttribute<SettingContent>();
                if (settingContent is null)
                {
                    continue;
                }

                sceneData.settingType = settingContent.type;
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

            PageScene pageScene = sceneMaps.Find(x => x.settingType == obj.GetType())?.scene;
            if (pageScene is null)
            {
                return;
            }

            SwitchScene(pageScene);
        }

        public static PageScene GetScene(Type type)
        {
            if (type.IsSubclassOf(typeof(PageScene)) is false)
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

        public static T GetScene<T>() where T : PageScene
        {
            return (T)GetScene(typeof(T));
        }

        public static void SwitchScene<T>() where T : PageScene
        {
            SwitchScene(typeof(T));
        }

        public static void SwitchScene(Type type)
        {
            //检查type是否实现了PageScene
            if (type.IsSubclassOf(typeof(PageScene)) is false)
            {
                return;
            }

            SwitchScene(GetScene(type));
        }

        public static void SwitchScene(PageScene scene)
        {
            if (scene is null)
            {
                return;
            }

            if (_docker.current is not null)
            {
                _docker.current.StopAllCoroutine();
                _docker.current.OnDisable();
            }

            _docker.current = scene;
            _docker.current.OnEnable();
            Refresh();
        }

        public static void StartCoroutine(IEnumerator enumerator)
        {
            instance.current.StartCoroutine(enumerator);
        }

        public static void Refresh()
        {
            _docker?.Repaint();
        }
    }
}
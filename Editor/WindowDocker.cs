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
    public partial class WindowDocker
    {
        private static WindowDocker _docker;
        private static List<PageDocker> sceneMaps;
        private static Dictionary<Type, Type> optionsTypeList;
        private static UnityEngine.Object openScriptableObject;

        class PageDocker
        {
            public bool show;
            public PageScene scene;
            public List<PageScene> childs;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptBuilderComplation()
        {
            sceneMaps = new List<PageDocker>();
            optionsTypeList = new Dictionary<Type, Type>();
            List<Type> types = AppDomain.CurrentDomain.GetAllTypes<PageScene>();
            foreach (var VARIABLE in types)
            {
                Options options = VARIABLE.GetCustomAttribute<Options>();
                if (options is null)
                {
                    continue;
                }

                optionsTypeList.Add(options.type, VARIABLE);
            }

            foreach (var VARIABLE in types)
            {
                BindScene editorScene = VARIABLE.GetCustomAttribute<BindScene>();
                if (editorScene is null || editorScene.parent is not null)
                {
                    continue;
                }

                sceneMaps.Add(new PageDocker() { scene = (PageScene)Activator.CreateInstance(VARIABLE), childs = new List<PageScene>() });
            }

            foreach (var VARIABLE in types)
            {
                BindScene subScene = VARIABLE.GetCustomAttribute<BindScene>();
                if (subScene is null || subScene.parent is null)
                {
                    continue;
                }

                PageDocker docker = sceneMaps.Find(x => x.scene.GetType() == subScene.parent);
                if (docker is null)
                {
                    continue;
                }

                docker.childs.Add((PageScene)Activator.CreateInstance(VARIABLE));
            }

            if (EditorWindow.HasOpenInstances<WindowDocker>() is false)
            {
                return;
            }

            if (openScriptableObject == null)
            {
                OpenScene(null);
                return;
            }

            OpenScene(openScriptableObject);
            openScriptableObject = null;
        }

        [MenuItem("Window/GameEditor %L")]
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

            if (optionsTypeList.TryGetValue(target.GetType(), out Type sceneType) is false)
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
                _docker = GetWindow<WindowDocker>(false, "编辑器", true);
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

            if (optionsTypeList.TryGetValue(obj.GetType(), out Type sceneType) is false)
            {
                return;
            }

            SwitchScene(sceneType);
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
                _docker.current.OnDisable();
            }

            _docker.current = scene;
            _docker.current.OnEnable();
            Refresh();
        }

        public static void SwitchScene<T>() where T : PageScene
        {
            SwitchScene(typeof(T));
        }

        public static void Refresh()
        {
            _docker?.Repaint();
        }

        public static void StartCoroutine(IEnumerator enumerator)
        {
            _docker.StartCoroutine(enumerator);
        }
    }
}
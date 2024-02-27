using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public partial class ToolsWindow
    {
        private static ToolsWindow _docker;
        private static List<ToolbarScene> sceneMaps;
        private static UnityEngine.Object openScriptableObject;
        private static event Func<Type, string, bool> OnOpenAssetCallback;

        [MenuItem("ZGame/Home %h")]
        static void BackupHome()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene("Assets/Startup.unity");
        }


        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptBuilderComplation()
        {
            sceneMaps = new List<ToolbarScene>();
            List<Type> types = AppDomain.CurrentDomain.GetAllSubClasses<ToolbarScene>();


            foreach (var VARIABLE in types)
            {
                PageConfig setting = VARIABLE.GetCustomAttribute<PageConfig>();

                if (setting is null || setting.parent is not null)
                {
                    continue;
                }

                sceneMaps.Add((ToolbarScene)Activator.CreateInstance(VARIABLE));
            }

            foreach (var VARIABLE in types)
            {
                PageConfig setting = VARIABLE.GetCustomAttribute<PageConfig>();
                if (setting is null || setting.parent is null)
                {
                    continue;
                }

                foreach (var VARIABLE2 in sceneMaps)
                {
                    if (VARIABLE2.GetType().Equals(setting.parent))
                    {
                        ToolbarScene toolbarScene = (ToolbarScene)Activator.CreateInstance(VARIABLE);
                        VARIABLE2.childs.Add(toolbarScene);
                        toolbarScene.parent = VARIABLE2;
                        toolbarScene.isPopup = setting.isPopup;
                        if (setting.configType is not null || setting.extension.IsNullOrEmpty() is false)
                        {
                            OnOpenAssetCallback += (assetType, extension) =>
                            {
                                if (assetType.Equals(setting.configType) || extension.Equals(setting.extension))
                                {
                                    SwitchScene(toolbarScene);
                                    return true;
                                }

                                return false;
                            };
                        }

                        break;
                    }
                }
            }

            if (EditorWindow.HasOpenInstances<ToolsWindow>() is false)
            {
                return;
            }

            OpenScene();
        }

        [OnOpenAsset()]
        static bool OnOpened(int id, int line)
        {
            UnityEngine.Object target = UnityEditor.EditorUtility.InstanceIDToObject(id);
            if (target == null)
            {
                return false;
            }

            if (OnOpenAssetCallback is null)
            {
                return false;
            }

            string fullPath = AssetDatabase.GetAssetPath(target);
            return OnOpenAssetCallback(target.GetType(), Path.GetExtension(fullPath));
        }

        private static ToolsWindow TryOpenManager()
        {
            return _docker = ToolsWindow.GetWindow<ToolsWindow>(false, "工具集", true);
        }


        [MenuItem("Tools/ZGame Editor %L")]
        static void OpenScene()
        {
            TryOpenManager();
            SwitchScene(sceneMaps.FirstOrDefault());
        }


        public static ToolbarScene GetScene(Type type)
        {
            if (type.IsSubclassOf(typeof(ToolbarScene)) is false)
            {
                return default;
            }

            foreach (var VARIABLE in sceneMaps)
            {
                if (VARIABLE.GetType().Equals(type))
                {
                    return VARIABLE;
                }

                ToolbarScene page = VARIABLE.childs.Find(x => x.GetType().Equals(type));
                if (page is null)
                {
                    continue;
                }

                return page;
            }

            return default;
        }

        public static T GetScene<T>() where T : ToolbarScene
        {
            return (T)GetScene(typeof(T));
        }

        public static void SwitchScene<T>(params object[] args) where T : ToolbarScene
        {
            SwitchScene(typeof(T), args);
        }

        public static void SwitchScene(Type type, params object[] args)
        {
            //检查type是否实现了PageScene
            if (type.IsSubclassOf(typeof(ToolbarScene)) is false)
            {
                return;
            }

            SwitchScene(GetScene(type), args);
        }

        public static void SwitchScene(ToolbarScene scene, params object[] args)
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

        public static void Refresh()
        {
            _docker?.Repaint();
        }
    }
}
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
    public partial class GameBaseEditorWindow
    {
        private static GameBaseEditorWindow _docker;
        private static List<GameSubEditorWindow> sceneMaps;
        private static UnityEngine.Object openScriptableObject;
        private static event Func<Type, string, bool> OnOpenAssetCallback;

        [MenuItem("ZGame/Home %h")]
        static void BackupHome()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene("Assets/Startup.unity");
        }

        [MenuItem("ZGame/Clear PlayerPrefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptBuilderComplation()
        {
            sceneMaps = new List<GameSubEditorWindow>();
            List<Type> types = AppDomain.CurrentDomain.GetAllSubClasses<GameSubEditorWindow>();


            foreach (var VARIABLE in types)
            {
                GameSubEditorWindowOptions setting = VARIABLE.GetCustomAttribute<GameSubEditorWindowOptions>();

                if (setting is null || setting.parent is not null)
                {
                    continue;
                }

                sceneMaps.Add((GameSubEditorWindow)Activator.CreateInstance(VARIABLE));
            }

            foreach (var VARIABLE in types)
            {
                GameSubEditorWindowOptions setting = VARIABLE.GetCustomAttribute<GameSubEditorWindowOptions>();
                if (setting is null || setting.parent is null)
                {
                    continue;
                }

                foreach (var VARIABLE2 in sceneMaps)
                {
                    if (VARIABLE2.GetType().Equals(setting.parent))
                    {
                        GameSubEditorWindow gameSubEditorWindow = (GameSubEditorWindow)Activator.CreateInstance(VARIABLE);
                        VARIABLE2.childs.Add(gameSubEditorWindow);
                        gameSubEditorWindow.parent = VARIABLE2;
                        gameSubEditorWindow.isPopup = setting.isPopup;
                        if (setting.configType is not null || setting.extension.IsNullOrEmpty() is false)
                        {
                            OnOpenAssetCallback += (assetType, extension) =>
                            {
                                if (assetType.Equals(setting.configType) || extension.Equals(setting.extension))
                                {
                                    SwitchScene(gameSubEditorWindow);
                                    return true;
                                }

                                return false;
                            };
                        }

                        break;
                    }
                }
            }

            if (EditorWindow.HasOpenInstances<GameBaseEditorWindow>() is false)
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

        private static GameBaseEditorWindow TryOpenManager()
        {
            return _docker = GameBaseEditorWindow.GetWindow<GameBaseEditorWindow>(false, "工具集", true);
        }


        [MenuItem("Tools/ZGame Editor %L")]
        static void OpenScene()
        {
            TryOpenManager();
            SwitchScene(sceneMaps.FirstOrDefault());
        }


        public static GameSubEditorWindow GetScene(Type type)
        {
            if (type.IsSubclassOf(typeof(GameSubEditorWindow)) is false)
            {
                return default;
            }

            foreach (var VARIABLE in sceneMaps)
            {
                if (VARIABLE.GetType().Equals(type))
                {
                    return VARIABLE;
                }

                GameSubEditorWindow page = VARIABLE.childs.Find(x => x.GetType().Equals(type));
                if (page is null)
                {
                    continue;
                }

                return page;
            }

            return default;
        }

        public static T GetScene<T>() where T : GameSubEditorWindow
        {
            return (T)GetScene(typeof(T));
        }

        public static void SwitchScene<T>(params object[] args) where T : GameSubEditorWindow
        {
            SwitchScene(typeof(T), args);
        }

        public static void SwitchScene(Type type, params object[] args)
        {
            //检查type是否实现了PageScene
            if (type.IsSubclassOf(typeof(GameSubEditorWindow)) is false)
            {
                return;
            }

            SwitchScene(GetScene(type), args);
        }

        public static void SwitchScene(GameSubEditorWindow scene, params object[] args)
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
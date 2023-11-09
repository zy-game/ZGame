using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public class ConfigBase : ScriptableObject
    {
    }

    public partial class Docker
    {
        private static Docker _docker;

        [MenuItem("Window/GameEditor %L")]
        public static void OpenScene()
        {
            OpenScene(null);
        }

        [OnOpenAsset()]
        public static bool OnOpened(int id, int line)
        {
            UnityEngine.Object target = UnityEditor.EditorUtility.InstanceIDToObject(id);
            if (typeof(ConfigBase).IsAssignableFrom(target.GetType()))
            {
                Docker.OpenScene(target);
                return true;
            }

            return false;
        }

        public static void OpenScene(Object obj)
        {
            if (_docker == null)
            {
                _docker = GetWindow<Docker>(false, "编辑器", true);
                if (_docker == null)
                {
                    return;
                }
            }

            _docker._scenes = new List<PageScene>();
            List<Type> types = AppDomain.CurrentDomain.GetAllTypes<PageScene>();
            foreach (var VARIABLE in types)
            {
                if (typeof(SubPageScene).IsAssignableFrom(VARIABLE))
                {
                    continue;
                }

                PageScene pageScene = (PageScene)Activator.CreateInstance(VARIABLE, new object[] { _docker });
                if (pageScene is null)
                {
                    continue;
                }

                _docker._scenes.Add(pageScene);
            }

            if (_docker._scenes.Count == 0)
            {
                return;
            }

            if (obj == null)
            {
                _docker.current = _docker._scenes.FirstOrDefault();
                return;
            }

            foreach (var VARIABLE in _docker._scenes)
            {
                PageScene pageScene = VARIABLE.OpenAssetObject(obj);
                if (pageScene is null)
                {
                    continue;
                }

                _docker.SwitchPageScene(pageScene);
            }
        }
    }
}
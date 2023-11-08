using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public partial class Docker
    {
        [MenuItem("Window/GameEditor")]
        public static void OpenScene()
        {
            OpenScene(null);
        }

        public static void OpenScene(Object obj)
        {
            Docker docker = GetWindow<Docker>(false, "编辑器", true);
            if (docker == null)
            {
                return;
            }

            docker._scenes = new List<PageScene>();
            List<Type> types = AppDomain.CurrentDomain.GetAllTypes<PageScene>();
            foreach (var VARIABLE in types)
            {
                if (typeof(SubPageScene).IsAssignableFrom(VARIABLE))
                {
                    continue;
                }

                PageScene pageScene = (PageScene)Activator.CreateInstance(VARIABLE, new object[] { docker });
                if (pageScene is null)
                {
                    continue;
                }

                docker._scenes.Add(pageScene);
            }

            if (docker._scenes.Count == 0)
            {
                return;
            }

            if (obj != null)
            {
                if (docker.map.TryGetValue(obj.GetType(), out Action<Object> call))
                {
                    call?.Invoke(obj);
                }

                return;
            }

            docker.current = docker._scenes.FirstOrDefault();
        }
    }
}
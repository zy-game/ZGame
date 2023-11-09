using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public abstract class PageScene : IDisposable
    {
        public abstract string name { get; }
        public Docker docker { get; }
        public PageScene parent { get; private set; }
        public List<SubPageScene> SubScenes { get; } = new List<SubPageScene>();
        public bool foldout;

        public PageScene(Docker window)
        {
            docker = window;
        }


        public virtual PageScene OpenAssetObject(Object obj)
        {
            foreach (var VARIABLE in SubScenes)
            {
                if (VARIABLE.OpenAssetObject(obj) is not null)
                {
                    return VARIABLE;
                }
            }

            return default;
        }

        public void RegisterSubPageScene<T>() where T : SubPageScene
        {
            T pageScene = (T)Activator.CreateInstance(typeof(T), new object[] { docker });
            if (pageScene is null)
            {
                return;
            }

            pageScene.parent = this;
            SubScenes.Add(pageScene);
        }

        public void RemoveSubPageScene<T>() where T : SubPageScene
        {
            T subPageScene = GetSubPageScene<T>();
            if (subPageScene is null)
            {
                return;
            }

            SubScenes.Remove(subPageScene);
        }

        public T GetSubPageScene<T>() where T : SubPageScene
        {
            return (T)SubScenes.Find(x => x.GetType().Equals(typeof(T)));
        }

        public virtual void Dispose()
        {
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnGUI(string search, Rect rect)
        {
        }

        public bool OnDrwaingMeunItem(float offset, float width)
        {
            Rect contains = EditorGUILayout.BeginVertical();
            docker.BeginColor(docker.current.Equals(this) ? Color.cyan : GUI.color);
            foldout = docker.MenuFoldout(foldout, name, SubScenes.Count > 0);
            docker.EndColor();
            GUILayout.Space(5);
            docker.BeginColor(docker.current.Equals(this) ? ZStyle.inColor : ZStyle.outColor);
            GUILayout.BeginHorizontal();
            GUILayout.Space(-offset);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.MaxWidth(width), GUILayout.Height(1));
            GUILayout.EndHorizontal();
            docker.EndColor();
            bool result = false;
            if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) &&
                Event.current.button == 0)
            {
                result = true;
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
            return result;
        }
    }
}
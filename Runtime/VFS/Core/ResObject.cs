using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.VFS
{
    public partial class ResObject : IReference
    {
        public static ResObject DEFAULT => new ResObject();
        private object source;
        private ResPackage parent;

        /// <summary>
        /// 资源名
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int refCount { get; private set; }

        /// <summary>
        /// 所属资源包
        /// </summary>
        private ResPackage Parent => parent;

        /// <summary>
        /// 加载是否成功
        /// </summary>
        /// <returns></returns>
        public bool IsSuccess()
        {
            if (source != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取指定类型的资源
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAsset<T>(GameObject gameObject = null) where T : Object
        {
            if (source == null)
            {
                Debug.Log("基础资源为空");
                return default;
            }

            Ref();
            gameObject?.SubscribeDestroyEvent(() => { ResObject.Unload(this); });
            if (typeof(T) == typeof(Sprite) && source is Texture2D texture2D)
            {
                source = Sprite.Create(texture2D, new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)), Vector2.one / 2);
            }

            return (T)source;
        }

        /// <summary>
        /// 实例化成游戏对象
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public GameObject Instantiate(Transform parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject gameObject = (GameObject)GameObject.Instantiate((Object)source);
            if (gameObject == null)
            {
                return gameObject;
            }

            gameObject.SubscribeDestroyEvent(() => { ResObject.Unload(this); });
            if (parent != null)
            {
                gameObject.SetParent(parent.transform, pos, rot, scale);
            }
            else
            {
                gameObject.SetParent(null, pos, rot, scale);
            }

            return gameObject;
        }


        public void Release()
        {
            Debug.Log("Dispose ResObject:" + name);
            refCount = 0;
            source = null;
            parent = null;
            name = String.Empty;
        }

        internal void Ref()
        {
            refCount++;
            parent?.Ref();
        }

        internal void Unref()
        {
            refCount--;
            parent?.Unref();
        }
    }

    public partial class ResObject
    {
        /// <summary>
        /// 缓存池列表
        /// </summary>
        private static List<ResObject> resObjects = new();

        /// <summary>
        /// 缓存池
        /// </summary>
        private static List<ResObject> resObjectCache = new();

#if UNITY_EDITOR
        static internal void OnDrawingGUI()
        {
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源对象", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < resObjects.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(resObjects[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(resObjects[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源对象池", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < resObjects.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(resObjects[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(resObjects[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
#endif

        /// <summary>
        /// 检查未引用的对象
        /// </summary>
        internal static void CheckUnusedRefObject()
        {
            for (int i = resObjects.Count - 1; i >= 0; i--)
            {
                if (resObjects[i].refCount > 0)
                {
                    continue;
                }

                resObjectCache.Add(resObjects[i]);
                resObjects.RemoveAt(i);
            }
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        internal static void ReleaseUnusedRefObject()
        {
            for (int i = resObjectCache.Count - 1; i >= 0; i--)
            {
                if (resObjectCache[i].refCount > 0)
                {
                    continue;
                }

                RefPooled.Release(resObjectCache[i]);
            }

            resObjectCache.Clear();
        }

        internal static bool TryGetValue(string path, out ResObject resObject)
        {
            resObject = resObjects.Find(x => x.name == path);
            if (resObject is null)
            {
                resObject = resObjectCache.Find(x => x.name == path);
                if (resObject is not null)
                {
                    resObjects.Add(resObject);
                    resObjectCache.Remove(resObject);
                }
            }

            return resObject is not null;
        }

        public static void Unload(ResObject resObject)
        {
            resObject.Unref();
            if (resObject.refCount > 0)
            {
                return;
            }

            resObjectCache.Add(resObject);
            resObjects.Remove(resObject);
        }

        internal static ResObject Create(ResPackage parent, object obj, string path)
        {
            if (obj == null)
            {
                throw new NullReferenceException(nameof(obj));
            }

            ResObject resObject = RefPooled.Spawner<ResObject>();
            resObject.source = obj;
            resObject.name = path;
            resObject.parent = parent;
            resObject.refCount = 1;
            resObjects.Add(resObject);
            return resObject;
        }
    }
}
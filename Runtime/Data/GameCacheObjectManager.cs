using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Data
{
    public class GameCacheObjectManager : GameFrameworkModule
    {
        /// <summary>
        /// 引用对象数据
        /// </summary>
        private List<IGameCacheObject> refMap = new();

        /// <summary>
        /// 缓存池列表
        /// </summary>
        private List<IGameCacheObject> cacheMap = new();

        private float time = 0;

        public override void OnAwake(params object[] args)
        {
            time = Time.realtimeSinceStartup;
        }

#if UNITY_EDITOR
        protected internal override void OnDrawingGUI()
        {
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("正在使用对象", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < refMap.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(refMap[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(refMap[i].GetType().Name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(refMap[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("未使用对象", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < cacheMap.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(cacheMap[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(cacheMap[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
#endif
        public override void Update()
        {
            if (Time.realtimeSinceStartup - time < ResConfig.instance.timeout)
            {
                return;
            }

            time = Time.realtimeSinceStartup;
            ReleaseUnusedRefObject();
            CheckUnusedRefObject();
        }

        /// <summary>
        /// 检查未引用的对象
        /// </summary>
        private void CheckUnusedRefObject()
        {
            for (int i = refMap.Count - 1; i >= 0; i--)
            {
                if (refMap[i].refCount > 0)
                {
                    continue;
                }

                cacheMap.Add(refMap[i]);
                refMap.RemoveAt(i);
            }
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        private void ReleaseUnusedRefObject()
        {
            for (int i = cacheMap.Count - 1; i >= 0; i--)
            {
                if (cacheMap[i].refCount > 0)
                {
                    continue;
                }

                cacheMap[i].Dispose();
            }

            cacheMap.Clear();
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            return refMap.Exists(x => x.name == key);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IGameCacheObject GetDatable(string key)
        {
            return refMap.Find(x => x.name == key);
        }

        /// <summary>
        /// 尝试获取指定的数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetValue<T>(string key, out T data)
        {
            data = (T)refMap.Find(x => x.name == key);
            return data != null;
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public void SetCacheData(IGameCacheObject data)
        {
            if (Contains(data.name))
            {
                throw new Exception($"{data.name}已存在");
            }

            refMap.Add(data);
        }

        /// <summary>
        /// 清理所有数据
        /// </summary>
        public void Clear()
        {
            refMap.Clear();
        }

        /// <summary>
        /// 清理指定数据
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            IGameCacheObject data = GetDatable(key);
            if (data is null)
            {
                return;
            }

            data.Dispose();
            refMap.Remove(data);
        }

        /// <summary>
        /// 获取所有相同类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] All<T>() where T : IGameCacheObject
        {
            List<T> list = new();
            foreach (var VARIABLE in refMap)
            {
                if (VARIABLE is T)
                {
                    list.Add((T)VARIABLE);
                }
            }

            return list.ToArray();
        }

        public override void Release()
        {
            foreach (var VARIABLE in refMap)
            {
                VARIABLE.Dispose();
            }

            refMap.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
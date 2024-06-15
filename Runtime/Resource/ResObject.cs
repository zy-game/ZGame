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

namespace ZGame.Resource
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

            refCount++;
            parent?.Ref();
            gameObject?.DestroyEvent(() =>
            {
                if (AppCore.Resource is null)
                {
                    return;
                }

                AppCore.Resource.Unload(this);
            });
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
            GameObject gameObject = Instantiate();
            if (gameObject == null)
            {
                return gameObject;
            }

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

        public GameObject Instantiate()
        {
            GameObject gameObject = GameObject.Instantiate(GetAsset<GameObject>());
            if (gameObject == null)
            {
                return gameObject;
            }

            gameObject.DestroyEvent(() =>
            {
                if (AppCore.Resource is null)
                {
                    return;
                }

                AppCore.Resource.Unload(this);
            });
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
    }
}
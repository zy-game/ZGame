using System;
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
    public sealed class ResObject : IGameCacheObject
    {
        private object obj;
        private ResPackage parent;

        public string name { get; private set; }

        public int refCount { get; private set; }

        public Object Asset => (Object)obj;

        public ResPackage Parent => parent;

        public bool IsSuccess()
        {
            if (obj != null)
            {
                return true;
            }

            return false;
        }

        public T GetAsset<T>(GameObject gameObject)
        {
            if (obj == null || obj is not Object)
            {
                return default;
            }

            Ref();
            gameObject?.RegisterGameObjectDestroyEvent(() => { Unref(); });
            return (T)obj;
        }

        public void Release()
        {
            Debug.Log("Dispose ResObject:" + name);
            obj = null;
            for (int i = 0; i < refCount; i++)
            {
                parent?.Unref();
            }

            refCount = 0;
            parent = null;
            name = String.Empty;
        }

        public void Ref()
        {
            refCount++;
            parent?.Ref();
        }

        public void Unref()
        {
            refCount--;
            parent?.Unref();
        }

        internal static ResObject Create(ResPackage parent, object obj, string path)
        {
            ResObject resObject = GameFrameworkFactory.Spawner<ResObject>();
            resObject.obj = obj;
            resObject.name = path;
            resObject.parent = parent;
            resObject.refCount = 0;
            return resObject;
        }

        public static ResObject Create(object obj, string path)
        {
            return Create(null, obj, path);
        }
    }
}
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
    public sealed class ResObject : IDisposable
    {
        private object obj;
        private string _path;
        private int _refCount;
        private ResPackage parent;

        public string path
        {
            get { return _path; }
        }

        public int refCount
        {
            get { return _refCount; }
        }

        public Object Asset
        {
            get { return (Object)obj; }
        }

        public ResPackage Parent
        {
            get { return parent; }
        }

        public bool IsSuccess()
        {
            if (obj != null)
            {
                return true;
            }

            return false;
        }

        public T GetAsset<T>()
        {
            if (obj == null)
            {
                return default;
            }

            parent?.Required();
            return (T)obj;
        }

        public void Release()
        {
            _refCount--;
            parent?.Unrequire();
        }

        public void Dispose()
        {
            Debug.Log("Dispose ResObject:" + path);
            obj = null;
            for (int i = 0; i < _refCount; i++)
            {
                parent?.Unrequire();
            }

            _refCount = 0;
            parent = null;
            _path = String.Empty;
            GC.SuppressFinalize(this);
        }

        internal static ResObject Create(ResPackage parent, object obj, string path)
        {
            ResObject resObject = new ResObject();
            resObject.obj = obj;
            resObject._path = path;
            resObject.parent = parent;
            resObject._refCount = 0;
            return resObject;
        }

        public static ResObject Create(Object obj, string path)
        {
            return Create(null, obj, path);
        }
    }
}
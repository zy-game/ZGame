using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.Language;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Resource
{
    partial class ResPackage : IReference
    {
        public string name { get; private set; }
        public int refCount { get; private set; }
        public ResPackage[] dependencies { get; private set; }

        private bool isDefault;
        private AssetBundle bundle;
        private float nextCheckTime;


        internal void SetDependencies(params ResPackage[] dependencies)
        {
            this.dependencies = dependencies;
        }

        public bool IsSuccess()
        {
            return bundle != null;
        }

        internal void Ref()
        {
            refCount++;
            if (dependencies is null || dependencies.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in dependencies)
            {
                VARIABLE.Ref();
            }
        }

        internal void Unref()
        {
            refCount--;
            if (dependencies is null || dependencies.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in dependencies)
            {
                VARIABLE.Unref();
            }
        }

        public Object LoadAsset(string path)
        {
            if (bundle is null)
            {
                return default;
            }

            return bundle.LoadAsset(path);
        }

        public async UniTask<Object> LoadAssetAsync(string path)
        {
            if (bundle is null)
            {
                return default;
            }

            return await bundle.LoadAssetAsync(path);
        }

        public T LoadAsset<T>(string path) where T : Object
        {
            if (bundle is null)
            {
                return default;
            }

            return bundle.LoadAsset<T>(path);
        }

        public async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        {
            if (bundle is null)
            {
                return default;
            }

            var result = await bundle.LoadAssetAsync<T>(path);
            return (T)result;
        }

        public void Release()
        {
            bundle?.Unload(true);
            Resources.UnloadUnusedAssets();
            Debug.Log("释放资源包:" + name);
        }
    }
}
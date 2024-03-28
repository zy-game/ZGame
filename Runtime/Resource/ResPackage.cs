using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class ResPackage : IGameCacheObject
    {
        public string name { get; }
        public AssetBundle bundle { get; }
        public int refCount { get; private set; }
        public ResPackage[] dependencies { get; private set; }

        private bool isDefault;
        private float nextCheckTime;

        internal ResPackage(string title)
        {
            this.name = title;
            this.isDefault = true;
        }

        internal ResPackage(AssetBundle bundle)
        {
            this.bundle = bundle;
            this.isDefault = false;
            this.name = bundle.name;
        }

        internal void SetDependencies(params ResPackage[] dependencies)
        {
            this.dependencies = dependencies;
            Debug.Log(name + " 设置引用资源包：" + string.Join(",", dependencies.Select(x => x.name).ToArray()));
        }

        public void Ref()
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

        public void Unref()
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

        public void Release()
        {
            bundle?.Unload(true);
            Resources.UnloadUnusedAssets();
            Debug.Log("释放资源包:" + name);
        }
    }
}
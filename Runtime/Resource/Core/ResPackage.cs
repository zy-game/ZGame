using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class ResPackage : IDisposable
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
            RefreshCheckTime();
        }

        internal ResPackage(AssetBundle bundle)
        {
            this.bundle = bundle;
            this.isDefault = false;
            this.name = bundle.name;
            RefreshCheckTime();
        }

        internal void SetDependencies(params ResPackage[] dependencies)
        {
            this.dependencies = dependencies;
            Debug.Log("设置引用资源包：" + string.Join(",", dependencies.Select(x => x.name).ToArray()));
        }

        internal void Required()
        {
            refCount++;
            RefreshCheckTime();
            if (dependencies is null || dependencies.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in dependencies)
            {
                VARIABLE.Required();
            }
        }

        internal void Unrequire()
        {
            refCount--;
            RefreshCheckTime();
            if (dependencies is null || dependencies.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in dependencies)
            {
                VARIABLE.Unrequire();
            }
        }

        internal bool CanUnloadPackage()
        {
            if (isDefault || refCount > 0 || Time.realtimeSinceStartup < nextCheckTime)
            {
                return false;
            }

            RefreshCheckTime();
            return true;
        }

        private void RefreshCheckTime()
        {
            nextCheckTime = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
        }

        public void Dispose()
        {
            ResObjectCache.instance.RemovePackage(this);
            bundle?.Unload(true);
            Resources.UnloadUnusedAssets();
            Debug.Log("释放资源包:" + name);
        }
    }
}
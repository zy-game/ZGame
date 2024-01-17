using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Window;

namespace ZGame.Resource
{
    public class PackageHandle : IDisposable
    {
        public string name { get; }
        public AssetBundle bundle { get; }
        public int refCount { get; private set; }
        public PackageHandle[] dependencies { get; private set; }

        private bool isDefault;
        private float nextCheckTime;

        internal PackageHandle(string title, bool isDefault)
        {
            this.name = title;
            this.isDefault = isDefault;
            RefreshCheckTime();
        }

        internal PackageHandle(AssetBundle bundle) : this(bundle.name, false)
        {
            this.bundle = bundle;
        }

        internal void SetDependencies(params PackageHandle[] dependencies)
        {
            this.dependencies = dependencies;
            Debug.Log("设置引用资源包：" + string.Join(",", dependencies.Select(x => x.name).ToArray()));
        }

        internal void AddRef()
        {
            refCount++;
            RefreshCheckTime();
            if (dependencies is null || dependencies.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in dependencies)
            {
                VARIABLE.AddRef();
            }
        }

        internal void MinusRef()
        {
            refCount--;
            RefreshCheckTime();
            if (dependencies is null || dependencies.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in dependencies)
            {
                VARIABLE.MinusRef();
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
            Debug.Log("释放资源包:" + name);
            ResObjectCache.instance.RemovePackage(this.name);
            bundle?.Unload(true);
            Resources.UnloadUnusedAssets();
        }
    }
}
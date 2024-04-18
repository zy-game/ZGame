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

namespace ZGame.VFS
{
    partial class ResPackage : IReference
    {
        public string name { get; private set; }
        public int refCount { get; private set; }
        public AssetBundle bundle { get; private set; }
        public ResPackage[] dependencies { get; private set; }

        private bool isDefault;
        private float nextCheckTime;


        internal void SetDependencies(params ResPackage[] dependencies)
        {
            this.dependencies = dependencies;
            Debug.Log(name + " 设置引用资源包：" + string.Join(",", dependencies.Select(x => x.name).ToArray()));
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

        public void Release()
        {
            bundle?.Unload(true);
            Resources.UnloadUnusedAssets();
            Debug.Log("释放资源包:" + name);
        }
    }

    partial class ResPackage
    {
        /// <summary>
        /// 已加载的包列表
        /// </summary>
        private static List<ResPackage> usegePackageList = new();

        /// <summary>
        /// 缓存池
        /// </summary>
        private static List<ResPackage> packageCache = new();

        /// <summary>
        /// 默认资源包
        /// </summary>
        public static ResPackage DEFAULT = ResPackage.Create("DEFAULT");

        static ResPackage Create(string title)
        {
            ResPackage self = RefPooled.Spawner<ResPackage>();
            self.name = title;
            self.isDefault = true;
            return self;
        }

        internal static ResPackage Create(AssetBundle bundle)
        {
            ResPackage self = RefPooled.Spawner<ResPackage>();
            self.bundle = bundle;
            self.isDefault = false;
            self.name = bundle.name;
            usegePackageList.Add(self);
            return self;
        }
#if UNITY_EDITOR
        internal static void OnDrawingGUI()
        {
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源包", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < usegePackageList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(usegePackageList[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(usegePackageList[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源包缓存池", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < usegePackageList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(usegePackageList[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(usegePackageList[i].refCount.ToString());
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
            for (int i = usegePackageList.Count - 1; i >= 0; i--)
            {
                if (usegePackageList[i].refCount > 0)
                {
                    continue;
                }

                packageCache.Add(usegePackageList[i]);
                usegePackageList.RemoveAt(i);
            }
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        internal static void ReleaseUnusedRefObject()
        {
            for (int i = packageCache.Count - 1; i >= 0; i--)
            {
                if (packageCache[i].refCount > 0)
                {
                    continue;
                }

                RefPooled.Release(packageCache[i]);
            }

            packageCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        internal static bool TryGetValue(string name, out ResPackage package)
        {
            package = usegePackageList.Find(x => x.name == name);
            if (package is null)
            {
                package = packageCache.Find(x => x.name == name);
                if (package is not null)
                {
                    packageCache.Remove(package);
                    usegePackageList.Add(package);
                }
            }

            return package is not null;
        }

        internal static void InitializedDependenciesPackage(params ResourcePackageManifest[] manifests)
        {
            for (int i = 0; i < manifests.Length; i++)
            {
                if (ResPackage.TryGetValue(manifests[i].name, out ResPackage target) is false)
                {
                    continue;
                }

                if (manifests[i].dependencies is null || manifests[i].dependencies.Length == 0)
                {
                    continue;
                }

                List<ResPackage> dependencies = new List<ResPackage>();
                foreach (var dependency in manifests[i].dependencies)
                {
                    if (ResPackage.TryGetValue(dependency, out ResPackage packageHandle) is false)
                    {
                        continue;
                    }

                    dependencies.Add(packageHandle);
                }

                target.SetDependencies(dependencies.ToArray());
            }
        }

        internal static void Unload(string name)
        {
            if (TryGetValue(name, out var package) is false)
            {
                return;
            }

            Unload(package);
        }

        internal static void Unload(ResPackage resPackage)
        {
            packageCache.Add(resPackage);
            usegePackageList.Remove(resPackage);
        }
    }
}
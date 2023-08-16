using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    class DefaultRequestAssetExecute<T> : IExecute where T : Object
    {
        public InternalRequestAssetExecuteResult<T> result { get; set; }

        public void Release()
        {
            result = null;
            GC.SuppressFinalize(this);
        }

        public void Execute(params object[] args)
        {
            result = Engine.Class.Loader<InternalRequestAssetExecuteResult<T>>();
            result.path = args[0].ToString();
            if (result.path.StartsWith("Resources"))
            {
                string temp = result.path.Substring("Resources/".Length);
                result.asset = Resources.Load<T>(temp);
                return;
            }
            else
            {
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.On && HotfixOptions.instance.useAsset == Switch.On)
                {
                    result.asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(result.path);
                    return;
                }
#endif
                RuntimeBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(result.path);
                if (manifest is null)
                {
                    Engine.Console.Error("Not Find The Asset Bundle Manifest");
                    return;
                }

                InternalRuntimeBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
                if (runtimeAssetBundleHandle is null)
                {
                    Engine.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                    return;
                }

                result.asset = runtimeAssetBundleHandle.Load<T>(result.path);
            }
        }
    }
}
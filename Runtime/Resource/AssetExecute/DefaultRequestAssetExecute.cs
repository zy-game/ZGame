using UnityEngine;

namespace ZEngine.Resource
{
    class DefaultRequestAssetExecute<T> : IExecute<RequestAssetResult<T>> where T : Object
    {
        private bool isBindObject = false;
        public string path { get; set; }
        public RequestAssetResult<T> result { get; private set; }

        public void Release()
        {
            isBindObject = false;
        }

        public void Execute(params object[] args)
        {
            path = args[0].ToString();
            if (path.StartsWith("Resources"))
            {
                string temp = path.Substring("Resources/".Length);
                result = RequestAssetResult<T>.Create(path, Resources.Load<T>(temp));
                return;
            }
            else
            {
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.On && HotfixOptions.instance.useAsset == Switch.On)
                {
                    result = RequestAssetResult<T>.Create(path, UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path));
                    return;
                }
#endif
                RuntimeBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
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

                result = RequestAssetResult<T>.Create(path, runtimeAssetBundleHandle.Load<T>(path));
            }
        }
    }
}
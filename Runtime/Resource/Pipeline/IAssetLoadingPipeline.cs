using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = System.Object;

namespace ZGame.Resource
{
    public interface IAssetLoadingPipeline : IDisposable
    {
        ResObject LoadAsset(string path);
        UniTask<ResObject> LoadAssetAsync(string path);
        void Release(ResObject obj);
    }

    public class NetworkResourceLoadingPipeline : IAssetLoadingPipeline
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ResObject LoadAsset(string path)
        {
            throw new NotImplementedException();
        }

        public UniTask<ResObject> LoadAssetAsync(string path)
        {
            throw new NotImplementedException();
        }

        public void Release(ResObject obj)
        {
            throw new NotImplementedException();
        }
    }

    public class AssetBundleResourceLoadingPipeline : IAssetLoadingPipeline
    {
        private List<RuntimeAssetBundleHandle> _handles = new List<RuntimeAssetBundleHandle>();
        private List<RuntimeAssetBundleHandle> _waitingUnloadAssetBundle = new List<RuntimeAssetBundleHandle>();


        public ResObject LoadAsset(string path)
        {
            throw new NotImplementedException();
        }

        public UniTask<ResObject> LoadAssetAsync(string path)
        {
            throw new NotImplementedException();
        }

        public void Release(ResObject obj)
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class ResourcesLoadingPipeline : IAssetLoadingPipeline
    {
        private List<ResObject> resources = new List<ResObject>();

        public ResObject LoadAsset(string path)
        {
            ResObject result = resources.Find(x => x.path.Equals(path));
            if (result is null)
            {
                result = new ResObject(Resources.Load(path.Substring(10)), path);
                resources.Add(result);
            }

            return result;
        }

        public async UniTask<ResObject> LoadAssetAsync(string path)
        {
            ResObject result = resources.Find(x => x.path.Equals(path));
            if (result is null)
            {
                result = new ResObject(await Resources.LoadAsync(path.Substring(10)), path);
                resources.Add(result);
            }

            return result;
        }

        public void Release(ResObject resObject)
        {
            ResObject _resObject = resources.Find(x => x.Equals(resObject));
            if (_resObject is null)
            {
                return;
            }

            _resObject.Release();
            if (_resObject.refCount > 0)
            {
                return;
            }

            _resObject.Dispose();
            resources.Remove(_resObject);
        }

        public void Dispose()
        {
            foreach (var VARIABLE in resources)
            {
                VARIABLE.Dispose();
            }

            resources.Clear();
            Resources.UnloadUnusedAssets();
        }
    }

    public class AssetDatabaseLoadingPipeline : IAssetLoadingPipeline
    {
        private List<ResObject> resources = new List<ResObject>();

        public ResObject LoadAsset(string path)
        {
            ResObject result = resources.Find(x => x.path.Equals(path));
            if (result is null)
            {
#if UNITY_EDITOR
                result = new ResObject(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Object)), path);
                resources.Add(result);
#endif
            }

            return result;
        }

        public async UniTask<ResObject> LoadAssetAsync(string path)
        {
            ResObject result = resources.Find(x => x.path.Equals(path));
            if (result is null)
            {
#if UNITY_EDITOR
                result = new ResObject(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Object)), path);
                await UniTask.Delay(1);
                resources.Add(result);
#endif
            }

            return result;
        }

        public void Release(ResObject resObject)
        {
            ResObject _resObject = resources.Find(x => x.Equals(resObject));
            if (_resObject is null)
            {
                return;
            }

            _resObject.Release();
            if (_resObject.refCount > 0)
            {
                return;
            }

            _resObject.Dispose();
            resources.Remove(_resObject);
        }

        public void Dispose()
        {
            foreach (var VARIABLE in resources)
            {
                VARIABLE.Dispose();
            }

            resources.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
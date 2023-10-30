using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public interface IPackageLoadingPipeline : IDisposable
    {
        UniTask<ErrorCode> LoadingPackageList(Action<float> progressCallback, params string[] args);
        UniTask<ErrorCode> LoadingModulePackageList(ResourceModuleManifest manifest, Action<float> progressCallback);
    }
}
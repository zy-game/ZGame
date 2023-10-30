using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public interface IPackageUpdatePipeline : IDisposable
    {
        UniTask<ErrorCode> StartUpdate(ResourceModuleManifest manifest, Action<float> progressCallback);
        UniTask<ErrorCode> StartUpdate(Action<float> progressCallback, params string[] args);
    }
}
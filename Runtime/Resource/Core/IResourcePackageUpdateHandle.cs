using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace ZGame.Resource
{
    public interface IResourcePackageUpdateHandle : IDisposable
    {
        UniTask UpdateResourcePackageList(List<ResourcePackageManifest> manifests);
    }
}
﻿using System;
using Cysharp.Threading.Tasks;
using ZGame.Window;

namespace ZGame.Resource
{
    public interface IResourceLoadingHandle : IDisposable
    {
        bool Contains(string path);
        ResHandle LoadAsset(string path);
        UniTask<ResHandle> LoadAssetAsync(string path);
        void Release(string path);
    }
}
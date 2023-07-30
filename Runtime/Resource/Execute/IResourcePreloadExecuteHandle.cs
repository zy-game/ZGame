﻿using System.Collections;

namespace ZEngine.Resource
{
    public interface IResourcePreloadExecuteHandle : IExecuteAsyncHandle<IResourcePreloadExecuteHandle>
    {
    }

    class DefaultResourcePreloadExecuteHandle : IResourcePreloadExecuteHandle
    {
        public void Subscribe(ISubscribe<IResourcePreloadExecuteHandle> subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }

        public bool EnsureExecuteSuccessfuly()
        {
            throw new System.NotImplementedException();
        }

        public void Execute(params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public float progress { get; }

        public IEnumerator GetCoroutine()
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(ISubscribe subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void Paused()
        {
            throw new System.NotImplementedException();
        }

        public void Resume()
        {
            throw new System.NotImplementedException();
        }

        public void Cancel()
        {
            throw new System.NotImplementedException();
        }
    }
}
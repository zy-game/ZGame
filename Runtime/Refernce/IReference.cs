using System;
using UnityEngine;
using ZGame.Resource;

namespace ZGame
{
    public interface ICloneable<T> : ICloneable where T : IReference
    {
        T Clone();
    }

    /// <summary>
    /// 游戏引用对象
    /// </summary>
    public interface IReference : IDisposable
    {
        /// <summary>
        /// 释放引用对象
        /// </summary>
        void IDisposable.Dispose()
        {
            if (this is ResObject resObject)
            {
                AppCore.Resource.Unload(resObject);
                return;
            }

            if (this is ResPackage resPackage)
            {
                AppCore.Resource.UnloadResPackage(resPackage.name);
                return;
            }

            RefPooled.Free(this);
        }

        /// <summary>
        /// 回收引用对象
        /// </summary>
        void Release();
    }
}
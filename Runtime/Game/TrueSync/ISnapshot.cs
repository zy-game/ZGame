using System;
using System.IO;
using FixMath.NET;

namespace ZGame.Game.LockStep
{
    /// <summary>
    /// 快照
    /// </summary>
    public interface ISnapshot : IReference
    {
        void Read(BinaryReader reader);
        void Write(BinaryWriter writer);
    }
}
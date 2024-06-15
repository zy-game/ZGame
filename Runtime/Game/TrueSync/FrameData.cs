using System;
using System.Collections.Generic;
using System.Linq;
using FixMath.NET;

namespace ZGame.Game.LockStep
{
    /// <summary>
    /// 帧数据
    /// </summary>
    public class FrameData : IReference, ICloneable<FrameData>
    {
        private long _frameID;
        private List<Command> _commands;

        public long frameID => _frameID;

        /// <summary>
        /// 当前帧所有的输入
        /// </summary>
        public List<Command> commands => _commands;

        public void Release()
        {
            this._frameID = 0;
            _commands.ForEach(RefPooled.Free);
            _commands.Clear();
        }

        /// <summary>
        /// 重置帧数据
        /// </summary>
        public void Reset()
        {
            this._frameID = 0;
            this._commands.ForEach(x => x.Clear());
        }

        public void SetFrameId(long id)
        {
            this._frameID = id;
        }

        /// <summary>
        /// 设置玩家的输入
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="command"></param>
        public void Set(uint uid, Command command)
        {
            if (command == null)
            {
                return;
            }

            _commands.RemoveAll(x => x.uid == uid);
            _commands.Add(command);
        }

        /// <summary>
        /// 获取玩家的输入
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Command Get(uint uid)
        {
            return _commands.FirstOrDefault(x => x.uid == uid);
        }

        public void CopyTo(FrameData frameData)
        {
            frameData._frameID = _frameID;
            frameData._commands = _commands.Select(x => x.Clone()).ToList();
        }

        public FrameData Clone()
        {
            FrameData frameData = RefPooled.Alloc<FrameData>();
            frameData._frameID = _frameID;
            frameData._commands = _commands.Select(x => x.Clone()).ToList();
            return frameData;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override int GetHashCode()
        {
            return _commands.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is not FrameData ohter)
            {
                return false;
            }

            return ohter == this;
        }

        public static bool operator !=(FrameData a, FrameData b)
        {
            return !(a == b);
        }

        public static bool operator ==(FrameData a, FrameData b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null)) return false;
            if (ReferenceEquals(b, null)) return false;
            return a._commands.SequenceEqual(b._commands);
        }

        public static FrameData Create(long id, List<Command> inputs)
        {
            FrameData frameData = RefPooled.Alloc<FrameData>();
            frameData._frameID = id;
            frameData._commands = inputs;
            return frameData;
        }

        public static FrameData Create(long id, IEnumerable<uint> racers)
        {
            FrameData frameData = RefPooled.Alloc<FrameData>();
            frameData._frameID = id;
            frameData._commands = racers.Select(x => Command.Create(x)).ToList();
            return frameData;
        }
    }
}
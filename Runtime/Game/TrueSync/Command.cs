using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FixMath.NET;
using Newtonsoft.Json;

namespace ZGame.Game.LockStep
{
    /// <summary>
    /// 玩家输入数据
    /// </summary>
    public class Command : IReference, ICloneable<Command>
    {
        /// <summary>
        /// 玩家id
        /// </summary>
        public uint uid;

        /// <summary>
        /// 输入数据列表
        /// </summary>
        public Dictionary<byte, double> inputs = new();

        public void Clear()
        {
            inputs.Clear();
        }

        public void Set(byte key, Fix64 value)
        {
            inputs[key] = value;
        }

        public Fix64 Get(byte key)
        {
            if (inputs.ContainsKey(key))
            {
                return inputs[key];
            }

            return 0;
        }

        public void Release()
        {
            uid = 0;
            Clear();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(uid); //写入玩家编号
            writer.Write(inputs.Count); //写入玩家输入数量
            foreach (var item in inputs)
            {
                writer.Write(item.Key);
                writer.Write(item.Value);
            }
        }

        public void Read(BinaryReader reader)
        {
            uid = reader.ReadUInt32(); //读取玩家编号
            int keyCount = reader.ReadInt32(); //读取玩家输入数量
            for (int j = 0; j < keyCount; j++)
            {
                var key = reader.ReadByte();
                var value = reader.ReadDouble();
                Set(key, value);
            }
        }

        public Command Clone()
        {
            Command command = RefPooled.Alloc<Command>();
            command.uid = uid;
            command.inputs = new Dictionary<byte, double>();
            foreach (var VARIABLE in inputs)
            {
                command.inputs.Add(VARIABLE.Key, VARIABLE.Value);
            }

            return command;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override int GetHashCode()
        {
            return uid.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is not Command ohter)
            {
                return false;
            }

            return ohter == this;
        }

        public static bool operator ==(Command a, Command b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null)) return false;
            if (ReferenceEquals(b, null)) return false;
            return a.uid == b.uid && a.inputs.SequenceEqual(b.inputs);
        }

        public static bool operator !=(Command a, Command b)
        {
            return !(a == b);
        }

        public static Command Create(uint uid)
        {
            Command command = RefPooled.Alloc<Command>();
            command.uid = uid;
            command.inputs = new();
            return command;
        }

        public static Command Create(uint uid, Dictionary<byte, double> inputs)
        {
            Command command = RefPooled.Alloc<Command>();
            command.uid = uid;
            command.inputs = inputs;
            return command;
        }
    }
}
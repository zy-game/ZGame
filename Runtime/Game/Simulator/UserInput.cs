using System.Collections.Generic;
using System.Text;
using FixMath.NET;

namespace ZGame.Game
{
    /// <summary>
    /// 玩家输入数据
    /// </summary>
    public class UserInput : IReference
    {
        /// <summary>
        /// 玩家id
        /// </summary>
        public uint uid;

        /// <summary>
        /// 输入数据列表
        /// </summary>
        public Dictionary<byte, Fix64> inputs = new();

        /// <summary>
        /// 获取输入
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Fix64 Get(byte id)
        {
            if (inputs.TryGetValue(id, out var value))
            {
                return value;
            }

            return default(Fix64);
        }

        /// <summary>
        /// 设置输入数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void Set(byte id, Fix64 value)
        {
            if (inputs.ContainsKey(id))
            {
                inputs[id] = value;
            }
            else
            {
                inputs.Add(id, value);
            }
        }


        public void Clear()
        {
            inputs.Clear();
        }


        public void Release()
        {
            uid = 0;
            Clear();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("uid:" + uid + " ");
            foreach (var item in inputs)
            {
                sb.Append($"{item.Key}:{item.Value} ");
            }

            return sb.ToString();
        }
    }
}
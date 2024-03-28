using System.Collections.Generic;

namespace ZGame.Language
{
    /// <summary>
    /// 默认语言定义
    /// </summary>
    class DefaultLanguageDefine : IConfigDatable
    {
        public int id { get; } = -1;

        private Dictionary<string, string> map = new();

        public bool HasLanguage(string filter)
        {
            return map.ContainsKey(filter);
        }

        public void SetLanguage(string filter, string value)
        {
            if (map.ContainsKey(filter))
            {
                map.Add(filter, value);
            }
            else
            {
                map[filter] = value;
            }
        }

        public string GetLanguage(string filter)
        {
            if (map.ContainsKey(filter))
            {
                return map[filter];
            }

            return "NOT FILTER:" + filter;
        }

        public bool Contains(string filter)
        {
            foreach (var VARIABLE in map.Values)
            {
                if (VARIABLE.Contains(filter))
                {
                    return true;
                }
            }

            return false;
        }

        public void Release()
        {
            map.Clear();
            map = null;
        }

        public bool Equals(string field, object value)
        {
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace ZGame.Editor.CodeGen
{
    class Description
    {
        private string desc;

        public Description(string desc)
        {
            this.desc = desc;
        }

        public string ToString(List<string> tab)
        {
            if (desc.IsNullOrEmpty())
            {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join("", tab) + "/// <summary>");
            sb.AppendLine(string.Join("", tab) + $"/// {desc}");
            sb.AppendLine(string.Join("", tab) + "/// </summary>");
            return sb.ToString();
        }
    }
}
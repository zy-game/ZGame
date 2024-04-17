using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZGame.Editor.CodeGen
{
    class ConstructorMethodCodeGeneric : IMethodCodeGen
    {
        private StringBuilder sb;
        private List<string> tab;
        private ParamsList paramsList;
        public string name { get; }
        public IClassCodeGen owner { get; }

        public ConstructorMethodCodeGeneric(IClassCodeGen parent, ParamsList paramsList)
        {
            this.name = parent.name;
            this.owner = parent;
            this.paramsList = paramsList;
            this.sb = new StringBuilder();
            this.tab = new List<string>();
            IClassCodeGen temp = parent;
            while (temp is not null)
            {
                this.tab.Add("\t");
                temp = temp.parent;
            }

            this.tab.Add("\t");
            this.sb.AppendLine(string.Join("", this.tab) + "public " + parent.name + "(" + paramsList is null ? ")" : paramsList?.ToString() + ")");
            this.sb.AppendLine(string.Join("", this.tab) + "{");
            this.tab.Add("\t");
        }

        public void BeginCodeScope(string code)
        {
            sb.AppendLine(string.Join("", this.tab) + code);
            sb.AppendLine(string.Join("", this.tab) + "{");
            this.tab.Add("\t");
        }

        public void EndCodeScope(string code = "")
        {
            this.tab.Remove(this.tab.LastOrDefault());
            sb.AppendLine(string.Join("", this.tab) + "}" + code);
            sb.AppendLine("");
        }

        public void WriterLine(string code)
        {
            sb.AppendLine(string.Join("", this.tab) + code);
        }

        public override string ToString()
        {
            this.tab.Remove(this.tab.LastOrDefault());
            sb.AppendLine(string.Join("", this.tab) + "}");
            sb.AppendLine("");
            return this.sb.ToString();
        }
    }
}
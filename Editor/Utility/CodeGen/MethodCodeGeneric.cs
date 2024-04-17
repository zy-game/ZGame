using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZGame.Editor.CodeGen
{
    class MethodCodeGeneric : IMethodCodeGen
    {
        private bool isStatic;
        private bool isOverride;
        private string returnType;
        private ACL access;
        private Description desc;
        private ParamsList paramsList;
        private StringBuilder sb;
        private List<string> tab;
        public string name { get; }
        public IClassCodeGen owner { get; }


        public MethodCodeGeneric(IClassCodeGen parent, string methodName, bool isStatic, bool isOverride, string desc, string returnType = "", ACL acl = ACL.Public, ParamsList paramsList = null)
        {
            this.owner = parent;
            this.name = methodName;
            this.isStatic = isStatic;
            this.isOverride = isOverride;
            this.returnType = returnType;
            this.access = acl;
            this.paramsList = paramsList;
            this.sb = new StringBuilder();
            this.tab = new List<string>();
            this.desc = new Description(desc);
            IClassCodeGen temp = parent;
            while (temp is not null)
            {
                this.tab.Add("\t");
                temp = temp.parent;
            }

            this.tab.Add("\t");

            string header = access switch
            {
                ACL.Public => "public ",
                ACL.Prottcted => "protected ",
                ACL.Private => "private ",
                ACL.Internal => "internal ",
                _ => throw new System.Exception("ACL Error"),
            };

            header += isStatic switch
            {
                true => "static ",
                false => "",
            };
            header += isOverride switch
            {
                true => "override ",
                false => "",
            };
            header += returnType switch
            {
                "" => "void ",
                _ => returnType + " ",
            };
            header += name + "(";
            header += paramsList is null ? "" : paramsList.ToString();
            header += ")";
            sb.AppendLine(this.desc.ToString(this.tab));
            sb.AppendLine(string.Join("", this.tab) + header);
            sb.AppendLine(string.Join("", this.tab) + "{");
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
            return sb.ToString();
        }
    }
}
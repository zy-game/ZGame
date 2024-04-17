using System.Collections.Generic;
using System.Text;

namespace ZGame.Editor.CodeGen
{
    class PropertyCodeGeneric : IPropertyCodeGen
    {
        private ACL acces;
        private string type;
        private Description desc;
        private List<string> tab;
        public string name { get; }
        public IClassCodeGen owner { get; }

        public void BeginCodeScope(string code)
        {
        }

        public void WriterLine(string code)
        {
        }

        public void EndCodeScope(string code = "")
        {
        }

        public PropertyCodeGeneric(IClassCodeGen parent, string propertyName, string desc, string propertyType, ACL acl)
        {
            this.name = propertyName;
            this.type = propertyType;
            this.acces = acl;
            this.owner = parent;
            this.desc = new Description(desc);
            this.tab = new List<string>();
            IClassCodeGen temp = parent;
            while (temp is not null)
            {
                this.tab.Add("\t");
                temp = temp.parent;
            }

            this.tab.Add("\t");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.desc.ToString(this.tab));
            sb.AppendLine($"{string.Join("", this.tab)}{acces.ToString().ToLower()} {type} {name} {{ get; set; }}\n");
            return sb.ToString();
        }
    }
}
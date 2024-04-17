using System.Collections.Generic;
using System.Text;

namespace ZGame.Editor.CodeGen
{
    class ClassCodeGeneric : IClassCodeGen
    {
        private ACL access;
        private List<string> tab;
        private string[] inheritList;
        private Description desc;
        private List<IMethodCodeGen> methods;
        private List<IClassCodeGen> subClassList;
        private List<IPropertyCodeGen> propertyList;
        public string name { get; }


        public IClassCodeGen parent { get; }


        public ClassCodeGeneric(string className, ACL acl, string desc, IClassCodeGen parent)
        {
            this.parent = parent;
            this.name = className;
            this.access = acl;
            this.methods = new List<IMethodCodeGen>();
            this.subClassList = new List<IClassCodeGen>();
            this.propertyList = new List<IPropertyCodeGen>();
            this.tab = new List<string>();
            this.desc = new Description(desc);
            IClassCodeGen temp = parent;
            while (temp is not null)
            {
                this.tab.Add("\t");
                temp = temp.parent;
            }

            this.tab.Add("\t");
        }

        public void SetInherit(params string[] args)
        {
            inheritList = args;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string inherit = inheritList is null ? "" : $" : {string.Join(", ", inheritList)}";
            sb.AppendLine(this.desc.ToString(this.tab));
            sb.AppendLine($"{string.Join("", this.tab)}{(access == ACL.Private ? "" : access.ToString().ToLower())} class {name} {inherit}");
            sb.AppendLine($"{string.Join("", this.tab)}{{");
            foreach (var VARIABLE in subClassList)
            {
                sb.AppendLine(VARIABLE.ToString());
            }

            foreach (var VARIABLE in propertyList)
            {
                sb.AppendLine(VARIABLE.ToString());
            }

            foreach (var VARIABLE in methods)
            {
                sb.AppendLine(VARIABLE.ToString());
            }

            sb.AppendLine(string.Join("", this.tab) + "}");
            sb.AppendLine("");
            return sb.ToString();
        }


        public IClassCodeGen AddClass(string className, string desc, ACL acl)
        {
            IClassCodeGen classCodeGen = new ClassCodeGeneric(className, acl, desc, this);
            subClassList.Add(classCodeGen);
            return classCodeGen;
        }

        public IMethodCodeGen AddConstructor(ParamsList args)
        {
            IMethodCodeGen methodCodeGen = new ConstructorMethodCodeGeneric(this, args);
            this.methods.Add(methodCodeGen);
            return methodCodeGen;
        }

        public void AddProperty(string propertyName, string propertyType, string desc, ACL acl = ACL.Public)
        {
            this.propertyList.Add(new PropertyCodeGeneric(this, propertyName, desc, propertyType, acl));
        }

        public IClassCodeGen SwitchClass(string className)
        {
            if (this.name == className)
            {
                return this;
            }

            IClassCodeGen classCodeGen = default;
            foreach (var VARIABLE in subClassList)
            {
                classCodeGen = VARIABLE.SwitchClass(className);
                if (classCodeGen is not null)
                {
                    break;
                }
            }

            return classCodeGen;
        }

        public IMethodCodeGen SwitchMethod(string methodName)
        {
            IMethodCodeGen methodCodeGen = this.methods.Find(x => x.name == methodName);
            if (methodCodeGen is not null)
            {
                return methodCodeGen;
            }

            foreach (var VARIABLE in subClassList)
            {
                methodCodeGen = VARIABLE.SwitchMethod(methodName);
                if (methodCodeGen is not null)
                {
                    break;
                }
            }

            return methodCodeGen;
        }


        public IMethodCodeGen AddMethod(string methodName, bool isStatic, bool isOverride, string desc, string returnType = "", ACL acl = ACL.Public, ParamsList paramsList = null)
        {
            IMethodCodeGen methodCodeGen = new MethodCodeGeneric(this, methodName, isStatic, isOverride, desc, returnType, acl, paramsList);
            this.methods.Add(methodCodeGen);
            return methodCodeGen;
        }

        public void BeginCodeScope(string code)
        {
        }

        public void EndCodeScope(string code = "")
        {
        }

        public void WriterLine(string code)
        {
        }
    }
}
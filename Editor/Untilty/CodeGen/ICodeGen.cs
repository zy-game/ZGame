using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZGame.Editor.CodeGen
{
    interface ICodeGen
    {
        string name { get; }
        void WriterLine(string code);
        void BeginCodeScope(string code);
        void EndCodeScope(string code = "");
    }

    interface IMethodCodeGen : ICodeGen
    {
        IClassCodeGen owner { get; }
    }

    interface IPropertyCodeGen : ICodeGen
    {
        IClassCodeGen owner { get; }
    }

    interface IClassCodeGen : ICodeGen
    {
        IClassCodeGen parent { get; }
        IClassCodeGen AddClass(string className, ACL acl);
        IMethodCodeGen AddConstructor(ParamsList args);
        IClassCodeGen SwitchClass(string className);
        IMethodCodeGen SwitchMethod(string methodName);
        void AddProperty(string propertyName, string propertyType, ACL acl = ACL.Public);
        IMethodCodeGen AddMethod(string methodName, bool isStatic, bool isOverride, string returnType = "", ACL acl = ACL.Public, ParamsList paramsList = null);
    }

    class MethodGener : IMethodCodeGen
    {
        private bool isStatic;
        private bool isOverride;
        private string returnType;
        private ACL access;
        private ParamsList paramsList;
        private StringBuilder sb;
        public string name { get; }
        public IClassCodeGen owner { get; }

        public MethodGener(IClassCodeGen parent, string methodName, bool isStatic, bool isOverride, string returnType = "", ACL acl = ACL.Public, ParamsList paramsList = null)
        {
            this.owner = parent;
            this.name = methodName;
            this.isStatic = isStatic;
            this.isOverride = isOverride;
            this.returnType = returnType;
            this.access = acl;
            this.paramsList = paramsList;
            this.sb = new StringBuilder();
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
            sb.AppendLine(header);
            sb.AppendLine("{");
        }

        public void BeginCodeScope(string code)
        {
            sb.AppendLine(code);
            sb.AppendLine("{");
        }

        public void EndCodeScope(string code = "")
        {
            sb.AppendLine("}" + code);
        }

        public void WriterLine(string code)
        {
            sb.AppendLine(code);
        }

        public override string ToString()
        {
            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    class ConstructorMethodGener : IMethodCodeGen
    {
        private StringBuilder sb;
        private ParamsList paramsList;
        public string name { get; }
        public IClassCodeGen owner { get; }

        public ConstructorMethodGener(IClassCodeGen parent, ParamsList paramsList)
        {
            this.name = parent.name;
            this.owner = parent;
            this.paramsList = paramsList;
            this.sb = new StringBuilder();
            this.sb.AppendLine("public " + parent.name + "(" + paramsList is null ? ")" : paramsList?.ToString() + ")");
            this.sb.AppendLine("{");
        }

        public void BeginCodeScope(string code)
        {
            sb.AppendLine(code);
            sb.AppendLine("{");
        }

        public void EndCodeScope(string code = "")
        {
            sb.AppendLine("}" + code);
        }

        public void WriterLine(string code)
        {
            sb.AppendLine(code);
        }

        public override string ToString()
        {
            this.sb.AppendLine("}");
            return this.sb.ToString();
        }
    }

    class PropertyGener : IPropertyCodeGen
    {
        private string type;
        private ACL acces;
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

        public PropertyGener(string propertyName, string propertyType, ACL acl)
        {
            this.name = propertyName;
            this.type = propertyType;
            this.acces = acl;
        }

        public override string ToString()
        {
            return $"{acces.ToString().ToLower()} {type} {name} {{ get; set; }}";
        }
    }

    class ClassGener : IClassCodeGen
    {
        private ACL access;
        private List<IMethodCodeGen> methods;
        private List<IClassCodeGen> subClassList;
        private List<IPropertyCodeGen> propertyList;
        public string name { get; }


        public IClassCodeGen parent { get; }


        public ClassGener(string className, ACL acl, IClassCodeGen parent)
        {
            this.parent = parent;
            this.name = className;
            this.access = acl;
            this.methods = new List<IMethodCodeGen>();
            this.subClassList = new List<IClassCodeGen>();
            this.propertyList = new List<IPropertyCodeGen>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{(access == ACL.Private ? "" : access.ToString().ToLower())} class {name}");
            sb.AppendLine("{");
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

            sb.AppendLine("}");
            return sb.ToString();
        }

        public IClassCodeGen AddClass(string className, ACL acl)
        {
            IClassCodeGen classCodeGen = new ClassGener(className, acl, this);
            subClassList.Add(classCodeGen);
            return classCodeGen;
        }

        public IMethodCodeGen AddConstructor(ParamsList args)
        {
            IMethodCodeGen methodCodeGen = new ConstructorMethodGener(this, args);
            this.methods.Add(methodCodeGen);
            return methodCodeGen;
        }

        public void AddProperty(string propertyName, string propertyType, ACL acl = ACL.Public)
        {
            this.propertyList.Add(new PropertyGener(propertyName, propertyType, acl));
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


        public IMethodCodeGen AddMethod(string methodName, bool isStatic, bool isOverride, string returnType = "", ACL acl = ACL.Public, ParamsList paramsList = null)
        {
            IMethodCodeGen methodCodeGen = new MethodGener(this, methodName, isStatic, isOverride, returnType, acl, paramsList);
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
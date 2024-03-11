using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPBehave;

namespace ZGame.Editor
{
    public class CodeGen
    {
        public enum ACL
        {
            PUBLIC,
            PRIVATE,
            INTERNAL,
            PROTECTED,
        }

        public class Params
        {
            public string type;
            public string name;

            public Params(string type, string name)
            {
                this.name = name;
                this.type = type;
            }
        }

        public class ParamsList
        {
            public static ParamsList None = new ParamsList();
            private List<Params> map = new();

            public ParamsList(params Params[] args)
            {
                map.AddRange(args);
            }

            public void SetParams(string type, string argsName)
            {
                if (map.Exists(x => x.name == argsName))
                {
                    throw new Exception($"params name is already exist:{argsName}");
                }

                map.Add(new Params(type, argsName));
            }

            public override string ToString()
            {
                string temp = string.Empty;
                for (int i = 0; i < map.Count; i++)
                {
                    temp += $"{map[i].type} {map[i].name}";
                    if (i < map.Count - 1)
                    {
                        temp += ", ";
                    }
                }

                return temp;
            }
        }


        interface ICodeGen
        {
        }

        interface IClassCodeGen : ICodeGen
        {
            IClassCodeGen AddClass(string className, ACL acl);
            ICodeGen AddConstructor(ParamsList args);
            void AddProperty(string propertyName, string propertyType, ACL acl = ACL.PUBLIC, string defaultValue = default);
        }

        interface IMethodCodeGen : ICodeGen
        {
        }

        interface IPropertyCodeGen : ICodeGen
        {
        }

        class ClassCodeGen : IClassCodeGen
        {
            private string className;

            private List<IMethodCodeGen> methods;
            private List<IClassCodeGen> subClassList;
            private List<IPropertyCodeGen> propertyList;

            public ClassCodeGen(string className)
            {
            }

            public IClassCodeGen AddClass(string className, ACL acl)
            {
                throw new System.NotImplementedException();
            }

            public ICodeGen AddConstructor(ParamsList args)
            {
                throw new System.NotImplementedException();
            }

            public void AddProperty(string propertyName, string propertyType, ACL acl = ACL.PUBLIC, string defaultValue = default)
            {
                throw new System.NotImplementedException();
            }
        }

        private ICodeGen current;
        private string nameSpace;
        private IClassCodeGen classCodeGem;
        private List<string> referenceNameSpace = new();


        public CodeGen(string className)
        {
            classCodeGem = new ClassCodeGen(className);
            current = classCodeGem;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            referenceNameSpace.ForEach(x => sb.AppendLine(x));
            sb.AppendLine(nameSpace);
            sb.AppendLine("{");
            sb.AppendLine(classCodeGem.ToString());
            sb.AppendLine("}");
            return sb.ToString();
        }

        public void SetNameSpace(string value)
        {
            nameSpace = $"namespace {value}";
        }

        public void AddReferenceNameSpace(params string[] value)
        {
            referenceNameSpace.AddRange(value);
        }

        public void SetProperty(string propertyName, string propertyType, ACL acl = ACL.PUBLIC, string defaultValue = default)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }

            classCodeGen.AddProperty(propertyName, propertyType, acl, defaultValue);
        }


        public void BeginConstructorMethod(ParamsList args)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }

            current = classCodeGen.AddConstructor(args);
        }

        public void EndConstructorMethod()
        {
            current = null;
        }

        public void BeginNestedClass(string nestedClassName, ACL acl = ACL.PUBLIC)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }

            current = classCodeGen.AddClass(nestedClassName, acl);
        }

        public void EndNestedClass()
        {
            current = null;
        }

        public void BeginMethod(string methodName, string returnType = "", ACL acl = ACL.PUBLIC, ParamsList paramsList = null)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }
        }

        public void EndMethod()
        {
            current = null;
        }

        public void BeginStaticMethod(string methodName, string returnType = "", ACL acl = ACL.PUBLIC, ParamsList paramsList = null)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }
        }

        public void EndStaticMethod()
        {
            current = null;
        }

        public void BeginOverrideMethod(string methodName, string returnType = "", ACL acl = ACL.PUBLIC, ParamsList paramsList = null)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }
        }

        public void EndOverrideMethod()
        {
            current = null;
        }

        public void BeginCode(string code)
        {
            if (current is not IMethodCodeGen methodCodeGen)
            {
                return;
            }
        }

        public void EndCode()
        {
            current = null;
        }

        public string GetLastValueName()
        {
            return string.Empty;
        }
    }
}
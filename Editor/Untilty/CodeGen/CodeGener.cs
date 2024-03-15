using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZGame.Editor.CodeGen
{
    public class CodeGener
    {
        private ICodeGen current;
        private string nameSpace;

        private IClassCodeGen classCodeGen;
        private List<string> referenceNameSpace = new();


        public CodeGener(string className)
        {
            classCodeGen = new ClassGener(className, ACL.Public, null);
            current = classCodeGen;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            referenceNameSpace.ForEach(x => sb.AppendLine(x));
            sb.AppendLine(nameSpace);
            sb.AppendLine("{");
            sb.AppendLine(classCodeGen.ToString());
            sb.AppendLine("}");
            return sb.ToString();
        }

        public void SetInherit(params string[] args)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }

            classCodeGen.SetInherit(args);
        }

        public void SetNameSpace(string value)
        {
            nameSpace = $"namespace {value}";
        }

        public void AddReferenceNameSpace(params string[] value)
        {
            foreach (var VARIABLE in value)
            {
                referenceNameSpace.Add($"using {VARIABLE};");
            }
        }

        public void SetProperty(string propertyName, string propertyType, ACL acl = ACL.Public)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }

            classCodeGen.AddProperty(propertyName, propertyType, acl);
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

        public void BeginClass(string nestedClassName, ACL acl = ACL.Public)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }

            current = classCodeGen.AddClass(nestedClassName, acl);
        }

        public void EndClass()
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }

            current = classCodeGen.parent;
        }

        public void SwitchClass(string className)
        {
            current = classCodeGen.SwitchClass(className);
        }

        public void SwitchMethod(string methodName)
        {
            current = classCodeGen.SwitchMethod(methodName);
        }

        public void BeginMethod(string methodName, bool isStatic, bool isOverride, string returnType = "", ACL acl = ACL.Public, ParamsList paramsList = null)
        {
            if (current is not IClassCodeGen classCodeGen)
            {
                return;
            }

            current = classCodeGen.AddMethod(methodName, isStatic, isOverride, returnType, acl, paramsList);
        }

        public void EndMethod()
        {
            if (current is not IMethodCodeGen methodCodeGen)
            {
                return;
            }

            current = methodCodeGen.owner;
        }

        public void WriteLine(string code)
        {
            current.WriterLine(code);
        }

        public void BeginCodeScope(string code)
        {
            current.BeginCodeScope(code);
        }

        public void EndCodeScope(string code = "")
        {
            current.EndCodeScope(code);
        }
    }
}
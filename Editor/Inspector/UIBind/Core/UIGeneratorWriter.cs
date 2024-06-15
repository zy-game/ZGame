using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZGame.Editor.UIBind
{
    public class UIGeneratorWriter : IReference
    {
        private string nameSpace;
        private ClassData root;
        private ClassData current;
        private bool isMethod;

        class ClassData
        {
            public string name;
            public ClassData parent;
            public List<ClassData> childs = new();
            public List<string> initCodeList = new();
            public List<string> fieldCodeList = new();
            public List<string> eventCodeList = new();
            public List<string> uninitCodeList = new();
            public List<string> methodCodeList = new();

            public ClassData(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                string tab = GetTables();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{tab}public class {name} : {typeof(ZGame.UI.UIBase).FullName}");
                sb.AppendLine($"{tab}{{");
                if (childs.Count > 0)
                {
                    childs.ForEach(x => sb.AppendLine(x.ToString()));
                }

                fieldCodeList.Sort((x, y) => x.Length.CompareTo(y.Length));
                fieldCodeList.ForEach(x => sb.AppendLine($"{tab}\t{x}"));
                sb.AppendLine($"{tab}\tpublic override void {nameof(ZGame.UI.UIBase.Awake)}()");
                sb.AppendLine($"{tab}\t{{");
                sb.AppendLine($"{tab}\t\tif(gameObject == null) return;");
                initCodeList.Sort((x, y) => x.Length.CompareTo(y.Length));
                initCodeList.ForEach(x => sb.AppendLine($"{tab}\t\t{x}"));
                eventCodeList.ForEach(x => sb.AppendLine($"{tab}\t\t{x}"));
                sb.AppendLine($"{tab}\t}}");

                sb.AppendLine($"{tab}\tpublic override void {nameof(ZGame.UI.UIBase.Release)}()");
                sb.AppendLine($"{tab}\t{{");

                uninitCodeList.Sort((x, y) => x.Length.CompareTo(y.Length));
                uninitCodeList.ForEach(x => sb.AppendLine($"{tab}\t\t{x}"));
                sb.AppendLine($"{tab}\t\tbase.{nameof(ZGame.UI.UIBase.Release)}();");
                sb.AppendLine($"{tab}\t}}");

                methodCodeList.ForEach(x => sb.AppendLine($"{tab}\t{x}"));
                sb.AppendLine($"{tab}}}");
                return sb.ToString();
            }

            private string GetTables()
            {
                ClassData temp = this;
                string tab = String.Empty;
                while (temp != null)
                {
                    tab += "\t";
                    temp = temp.parent;
                }

                return tab;
            }
        }


        public void Release()
        {
        }

        public void SetField(string s)
        {
            current.fieldCodeList.Add(s);
        }

        public void SetInit(string s)
        {
            current.initCodeList.Add(s);
        }

        public void SetEvent(string s)
        {
            current.eventCodeList.Add(s);
        }

        public void SetRelease(string s)
        {
            current.uninitCodeList.Add(s);
        }

        public void SetMethod(string s)
        {
            isMethod = true;
            current.methodCodeList.Add(s);
            current.methodCodeList.Add("{");
        }

        public void EndMethod()
        {
            isMethod = false;
            current.methodCodeList.Add("}");
        }

        public void WriteLine(string s)
        {
            if (isMethod)
            {
                current.methodCodeList.Add($"\t{s}");
            }
        }

        public void EndClass()
        {
            current = current.parent;
        }

        public void SetClass(string dataName)
        {
            if (root is null)
            {
                current = root = new ClassData(dataName);
                return;
            }

            var classData = new ClassData(dataName);
            current.childs.Add(classData);
            classData.parent = current;
            current = classData;
        }

        public void SetNameSpace(string docmentNameSpace)
        {
            this.nameSpace = docmentNameSpace;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {nameSpace}");
            sb.AppendLine("{");
            sb.AppendLine(root.ToString());
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string GetOverlapCode(string assetPath, ZGame.UI.UIDocment docment)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {nameSpace}");
            sb.AppendLine("{");
            sb.AppendLine($"\t[{typeof(RefPath).FullName}(\"{assetPath}\")]");
            sb.AppendLine("\tpublic class UICode_" + docment.RealName + " : UIBind_" + docment.RealName);
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tpublic override void Awake()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Awake();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tpublic override void Start(params object[] args)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Start(args);");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tpublic override void Enable()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Enable();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tpublic override void Disable()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Disable();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tpublic override void Release()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tbase.Release();");
            sb.AppendLine("\t\t}");
            root.methodCodeList.ForEach(x =>
            {
                if (x.Contains("virtual"))
                {
                    sb.AppendLine("\t\t" + x.Replace("virtual", "override"));
                    sb.AppendLine("\t\t{");
                    sb.AppendLine("\t\t}");
                }
            });
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    public sealed class UICodeGenerator : IUICodeGenerator
    {
        public UIDocment docment { get; private set; }

        public void Dispose()
        {
        }

        public void Execute(UI.UIDocment docment, UIGeneratorWriter writer)
        {
            this.docment = docment;
            if (docment.transform.parent != null)
            {
                writer.SetClass($"{docment.RealName}");
            }
            else
            {
                if (docment.NameSpace.IsNullOrEmpty())
                {
                    EditorUtility.DisplayDialog("UI 绑定错误", "请设置命名空间", "确定");
                    return;
                }

                if (docment.output == null)
                {
                    EditorUtility.DisplayDialog("UI 绑定错误", "请设置输出路径", "确定");
                    return;
                }

                writer.SetNameSpace(docment.NameSpace);
                writer.SetClass($"UIBind_{docment.RealName}");
            }

            Dictionary<Type, List<UIGeneratorAttribute>> genericTypeList = AppDomain.CurrentDomain.GetCustomAttributeMapList<UIGeneratorAttribute>();
            foreach (var VARIABLE in docment.bindList)
            {
                foreach (var generic in genericTypeList)
                {
                    foreach (var componentType in generic.Value)
                    {
                        if (componentType.componentType != typeof(UnityEngine.GameObject) && VARIABLE.TryGetComponent(componentType.componentType, out var component) is false)
                        {
                            continue;
                        }


                        IUIComponentGenerator genericComponent = (IUIComponentGenerator)Activator.CreateInstance(generic.Key);
                        if (genericComponent is null)
                        {
                            continue;
                        }

                        genericComponent.Execute(this, VARIABLE, writer);
                    }
                }
            }

            writer.EndClass();
        }
    }
}
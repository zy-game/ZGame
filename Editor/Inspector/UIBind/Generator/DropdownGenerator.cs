using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.UI.Dropdown))]
    public class DropdownGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField($"public {typeof(UnityEngine.UI.Dropdown).FullName} dropdown_{gameObject.name}{{ get; private set; }}");
            writer.SetInit($"dropdown_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(UnityEngine.UI.Dropdown).FullName}>();");
            writer.SetRelease($"dropdown_{gameObject.name} = null;");
            writer.SetEvent($"dropdown_{gameObject.name}.onValueChanged.RemoveAllListeners();");
            writer.SetEvent($"dropdown_{gameObject.name}.onValueChanged.AddListener(on_handle_{gameObject.name}_value_changed);");
            writer.SetMethod($"protected virtual void on_handle_{gameObject.name}_value_changed(int value)");
            writer.WriteLine($"// TODO: Implement on_handle_{gameObject.name}_value_changed");
            writer.EndMethod();
        }
    }
}
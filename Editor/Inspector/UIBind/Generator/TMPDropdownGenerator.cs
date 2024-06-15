using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(TMPro.TMP_Dropdown))]
    public class TMPDropdownGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField($"public {typeof(TMPro.TMP_Dropdown).FullName} dropdown_{gameObject.name} {{ get; private set; }}");
            writer.SetInit($"dropdown_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(TMPro.TMP_Dropdown).FullName}>();");
            writer.SetEvent($"dropdown_{gameObject.name}.onValueChanged.RemoveAllListeners();");
            writer.SetEvent($"dropdown_{gameObject.name}.onValueChanged.AddListener(on_handle_{gameObject.name}_value_changed);");
            writer.SetRelease($"dropdown_{gameObject.name} = null;");
            writer.SetMethod($"protected virtual void on_handle_{gameObject.name}_value_changed(int value)");
            writer.WriteLine($"// TODO: Implement on_handle_{gameObject.name}_value_changed");
            writer.EndMethod();
        }
    }
}
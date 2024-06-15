using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.UI.ToggleGroup))]
    public class ToggleGroupGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField((($"public {typeof(UnityEngine.UI.ToggleGroup).FullName} tgr_{gameObject.name} {{ get; private set; }}")));
            writer.SetInit((($"tgr_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(UnityEngine.UI.ToggleGroup).FullName}>();")));
            writer.SetRelease((($"tgr_{gameObject.name} = null;")));
            writer.SetMethod(($"protected virtual void on_handle_{gameObject.name}_switch({typeof(UnityEngine.UI.Toggle).FullName} toggle)"));
            writer.WriteLine(($"// TODO: Implement on_handle_{gameObject.name}_switch"));
            writer.EndMethod();
        }
    }
}
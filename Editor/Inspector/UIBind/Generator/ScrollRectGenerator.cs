using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.UI.ScrollRect))]
    public class ScrollRectGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField(($"public {typeof(UnityEngine.UI.ScrollRect).FullName} scr_{gameObject.name} {{ get; private set; }}"));
            writer.SetInit(($"scr_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(UnityEngine.UI.ScrollRect).FullName}>();"));
            writer.SetEvent($"scr_{gameObject.name}.onValueChanged.RemoveAllListeners();");
            writer.SetEvent(($"scr_{gameObject.name}.onValueChanged.AddListener(on_handle_{gameObject.name}_value_changed);"));
            writer.SetRelease(($"scr_{gameObject.name} = null;"));
            writer.SetMethod(($"protected virtual void on_handle_{gameObject.name}_value_changed(Vector2 value)"));
            writer.WriteLine(($"// TODO: Implement on_handle_{gameObject.name}_value_changed"));
            writer.EndMethod();
        }
    }
}
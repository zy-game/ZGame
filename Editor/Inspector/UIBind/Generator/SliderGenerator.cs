using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.UI.Slider))]
    public class SliderGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField(($"public {typeof(UnityEngine.UI.Slider).FullName} slider_{gameObject.name} {{ get; private set; }}"));
            writer.SetInit(($"slider_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(UnityEngine.UI.Slider).FullName}>();"));
            writer.SetEvent($"slider_{gameObject.name}.onValueChanged.RemoveAllListeners();");
            writer.SetEvent(($"slider_{gameObject.name}.onValueChanged.AddListener(on_handle_{gameObject.name}_value_changed);"));
            writer.SetRelease(($"slider_{gameObject.name} = null;"));
            writer.SetMethod(($"protected virtual void on_handle_{gameObject.name}_value_changed(float value)"));
            writer.WriteLine(($"// TODO: Implement on_handle_{gameObject.name}_value_changed"));
            writer.EndMethod();
        }
    }
}
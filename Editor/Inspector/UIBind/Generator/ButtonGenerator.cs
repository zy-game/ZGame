using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.UI.Button))]
    public class ButtonGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField($"public {typeof(UnityEngine.UI.Button).FullName} btn_{gameObject.name} {{ get; private set; }}");
            writer.SetInit($"btn_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(UnityEngine.UI.Button).FullName}>();");
            writer.SetEvent($"btn_{gameObject.name}.onClick.RemoveAllListeners();");
            writer.SetEvent($"btn_{gameObject.name}.onClick.AddListener(on_handle_{gameObject.name}_OnClick);");
            writer.SetRelease($"btn_{gameObject.name} = null;");
            writer.SetMethod($"protected virtual void on_handle_{gameObject.name}_OnClick()");
            writer.WriteLine($"// TODO: Implement on_handle_{gameObject.name}_value_changed");
            writer.EndMethod();
        }
    }
}
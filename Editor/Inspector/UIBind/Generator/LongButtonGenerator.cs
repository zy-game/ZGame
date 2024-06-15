using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(ZGame.UI.LongPresseButton))]
    public class LongButtonGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField($"public {typeof(ZGame.UI.LongPresseButton).FullName} lpb_{gameObject.name} {{ get; private set; }}");
            writer.SetInit($"lpb_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(ZGame.UI.LongPresseButton).FullName}>();");
            writer.SetEvent($"lpb_{gameObject.name}.onClick.RemoveAllListeners();");
            writer.SetEvent($"lpb_{gameObject.name}.onCancel.RemoveAllListeners();");
            writer.SetEvent($"lpb_{gameObject.name}.onUp.RemoveAllListeners();");
            writer.SetEvent($"lpb_{gameObject.name}.onDown.RemoveAllListeners();");
            writer.SetEvent($"lpb_{gameObject.name}.onClick.AddListener(on_handle_{gameObject.name}_click);");
            writer.SetEvent($"lpb_{gameObject.name}.onCancel.AddListener(on_handle_{gameObject.name}_cancel);");
            writer.SetEvent($"lpb_{gameObject.name}.onUp.AddListener(on_handle_{gameObject.name}_mouseUp);");
            writer.SetEvent($"lpb_{gameObject.name}.onDown.AddListener(on_handle_{gameObject.name}_mouseDown);");
            writer.SetRelease($"lpb_{gameObject.name} = null;");
            writer.SetMethod($"protected virtual void on_handle_{gameObject.name}_click()");
            writer.WriteLine($"// TODO: Implement on_handle_{gameObject.name}_value_changed");
            writer.EndMethod();
            writer.SetMethod($"protected virtual void on_handle_{gameObject.name}_mouseDown()");
            writer.WriteLine($"// TODO: Implement on_handle_{gameObject.name}_value_changed");
            writer.EndMethod();
            writer.SetMethod($"protected virtual void on_handle_{gameObject.name}_mouseUp()");
            writer.WriteLine($"// TODO: Implement on_handle_{gameObject.name}_value_changed");
            writer.EndMethod();
            writer.SetMethod($"protected virtual void on_handle_{gameObject.name}_cancel()");
            writer.WriteLine($"// TODO: Implement on_handle_{gameObject.name}_value_changed");
            writer.EndMethod();
        }
    }
}
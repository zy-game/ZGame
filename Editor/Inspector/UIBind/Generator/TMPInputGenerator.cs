using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(TMPro.TMP_InputField))]
    public class TMPInputGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField(($"public {typeof(TMPro.TMP_InputField).FullName} ipt_{gameObject.name} {{ get; private set; }}"));
            writer.SetInit(($"ipt_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(TMPro.TMP_InputField).FullName}>();"));
            writer.SetEvent($"ipt_{gameObject.name}.onValueChanged.RemoveAllListeners();");
            writer.SetEvent(($"ipt_{gameObject.name}.onValueChanged.AddListener(on_handle_{gameObject.name}_value_changed);"));
            writer.SetEvent($"ipt_{gameObject.name}.onSubmit.RemoveAllListeners();");
            writer.SetEvent(($"ipt_{gameObject.name}.onSubmit.AddListener(on_handle_{gameObject.name}_submit);"));
            writer.SetRelease(($"ipt_{gameObject.name} = null;"));
            writer.SetMethod(($"protected virtual void on_handle_{gameObject.name}_value_changed(string value)"));
            writer.WriteLine(($"// TODO: Implement on_handle_{gameObject.name}_value_changed"));
            writer.EndMethod();
            writer.SetMethod(($"protected virtual void on_handle_{gameObject.name}_submit(string value)"));
            writer.WriteLine(($"// TODO: Implement on_handle_{gameObject.name}_submit"));
            writer.EndMethod();
        }
    }
}
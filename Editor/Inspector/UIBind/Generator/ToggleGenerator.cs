using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.UI.Toggle))]
    public class ToggleGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField(($"public {typeof(UnityEngine.UI.Toggle).FullName} tg_{gameObject.name} {{ get; private set; }}"));
            writer.SetInit(($"tg_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(UnityEngine.UI.Toggle).FullName}>();"));
            writer.SetRelease(($"tg_{gameObject.name} = null;"));
            UnityEngine.UI.ToggleGroup group = gameObject.GetComponent<UnityEngine.UI.ToggleGroup>();
            if (group != null)
            {
                writer.SetEvent($"tg_{gameObject.name}.onValueChanged.RemoveAllListeners();");
                writer.SetInit(($"tg_{gameObject.name}.onValueChanged.AddListener(() => {{ on_handle_{group.name}_switch(tg_{gameObject.name}); }});"));
                return;
            }
            writer.SetEvent($"tg_{gameObject.name}.onValueChanged.RemoveAllListeners();");
            writer.SetEvent(($"tg_{gameObject.name}.onValueChanged.AddListener(on_handle_{gameObject.name}_value_changed);"));
            writer.SetMethod($"protected virtual void on_handle_{gameObject.name}_value_changed(bool value)");
            writer.WriteLine($"// TODO: Implement on_handle_{gameObject.name}_value_changed");
            writer.EndMethod();
        }
    }
}
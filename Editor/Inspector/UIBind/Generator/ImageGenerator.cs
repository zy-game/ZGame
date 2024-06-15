using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.UI.Image))]
    public class ImageGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField($"public {typeof(UnityEngine.UI.Image).FullName} img_{gameObject.name} {{ get; private set; }}");
            writer.SetInit($"img_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(UnityEngine.UI.Image).FullName}>();");
            writer.SetRelease($"img_{gameObject.name} = null;");
        }
    }
}
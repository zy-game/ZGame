using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.UI.RawImage))]
    public class RawImageGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField(($"public {typeof(UnityEngine.UI.RawImage).FullName} raw_{gameObject.name}{{ get; private set; }}"));
            writer.SetInit(($"raw_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(UnityEngine.UI.RawImage).FullName}>();"));
            writer.SetRelease(($"raw_{gameObject.name} = null;"));
        }
    }
}
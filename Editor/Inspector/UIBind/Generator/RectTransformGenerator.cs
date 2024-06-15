using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.RectTransform))]
    public class RectTransformGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField(($"public {typeof(UnityEngine.RectTransform).FullName} rt_{gameObject.name} {{ get; private set; }}"));
            writer.SetInit(($"rt_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(UnityEngine.RectTransform).FullName}>();"));
            writer.SetRelease(($"rt_{gameObject.name} = null;"));
        }
    }
}
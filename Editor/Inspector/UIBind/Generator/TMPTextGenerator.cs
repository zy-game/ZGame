using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(TMPro.TMP_Text))]
    public class TMPTextGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField(($"public {typeof(TMPro.TMP_Text).FullName} txt_{gameObject.name} {{ get; private set; }}"));
            writer.SetInit(($"txt_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(TMPro.TMP_Text).FullName}>();"));
            writer.SetRelease(($"txt_{gameObject.name} = null;"));
        }
    }
}
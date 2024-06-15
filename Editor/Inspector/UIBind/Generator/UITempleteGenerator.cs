using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(ZGame.UI.UIDocment))]
    public class UITempleteGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            var docment = gameObject.GetComponent<ZGame.UI.UIDocment>();
            writer.SetField((($"public {docment.RealName} tmp_{gameObject.name} {{ get; private set; }}")));
            writer.SetInit((($"tmp_{gameObject.name} = Create<{docment.RealName}>(this.go_{gameObject.name}, null);")));
            writer.SetRelease((($"tmp_{gameObject.name} = null;")));
            using (UICodeGenerator uiCodeGenerator = new UICodeGenerator())
            {
                uiCodeGenerator.Execute(docment, writer);
            }
        }
    }
}
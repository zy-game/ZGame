using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(ZGame.UI.LoopScrollViewer))]
    public class LoopViewGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField(($"public {typeof(ZGame.UI.LoopScrollViewer).FullName} lsv_{gameObject.name} {{ get; private set; }}"));
            writer.SetInit(($"lsv_{gameObject.name} = this.go_{gameObject.name}.GetComponent<{typeof(ZGame.UI.LoopScrollViewer).FullName}>();"));
            writer.SetEvent($"lsv_{gameObject.name}.onCellViewRefreshed.RemoveAllListeners();");
            writer.SetEvent($"lsv_{gameObject.name}.onCellViewRefreshed.AddListener(on_handle_{gameObject.name}_Reload);");
            writer.SetRelease(($"lsv_{gameObject.name} = null;"));
            writer.SetMethod(($"protected virtual void on_handle_{gameObject.name}_Reload({typeof(ZGame.UI.LoopScrollCellView).FullName} viewDataItem)"));
            writer.WriteLine(($"// TODO: Implement on_handle_{gameObject.name}_Reload"));
            writer.EndMethod();
        }
    }
}
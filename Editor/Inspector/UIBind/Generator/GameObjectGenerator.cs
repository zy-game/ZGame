using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    [UIGenerator(typeof(UnityEngine.GameObject))]
    public class GameObjectGenerator : IUIComponentGenerator
    {
        public void Dispose()
        {
        }

        public void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer)
        {
            writer.SetField($"public {typeof(UnityEngine.GameObject).FullName} go_{gameObject.name}{{ get; private set; }}");
            writer.SetInit($"go_{gameObject.name} = this.transform.Find(\"{gameObject.transform.GetPathWithToRoot(parent.docment.transform)}\").gameObject;");
            writer.SetRelease($"go_{gameObject.name} = null;");
        }
    }
}
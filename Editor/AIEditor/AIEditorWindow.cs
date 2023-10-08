using UnityEditor;
using ZEngine.Editor;

namespace ZEngine.Editor.AIEditor
{
    public class AIEditorWindow : EngineEditorWindow
    {
        // [MenuItem("工具/编辑器/AI编辑器")]
        public static void Open()
        {
            GetWindow<AIEditorWindow>(false, "行为编辑器", true);
        }

        protected override MenuListItem[] GetMenuList()
        {
            return default;
        }

        protected override void Actived()
        {
        }

        protected override void CreateNewItem()
        {
        }

        protected override void DrawingItemDataView(object data, float width)
        {
        }

        protected override void SaveChanged()
        {
        }
    }
}
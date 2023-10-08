using ZEngine.Editor;

namespace ZEngine.Editor.UIEditor
{
    [ProjectConfig]
    public class UIEditorOptions : ConfigScriptableObject<UIEditorOptions>
    {
    }

    public class UIEditorWindow : EngineEditorWindow
    {
        public static void Open()
        {
            GetWindow<UIEditorWindow>(false, "UI编辑器", true);
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
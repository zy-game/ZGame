using ZEngine.Editor;

namespace ZEngine.Editor.UIEditor
{
    [ProjectConfig]
    public class UIEditorOptions : SingleScript<UIEditorOptions>
    {
    }

    public class UIEditorWindow : EngineCustomEditor
    {
        public static void Open()
        {
            GetWindow<UIEditorWindow>(false, "UI编辑器", true);
        }

        protected override void Actived()
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateNewItem()
        {
            throw new System.NotImplementedException();
        }

        protected override void DrawingItemDataView(object data, float width)
        {
            throw new System.NotImplementedException();
        }

        protected override void SaveChanged()
        {
            throw new System.NotImplementedException();
        }
    }
}
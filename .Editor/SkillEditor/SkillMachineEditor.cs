using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ZEngine.Editor.SkillEditor
{
    class SkillMachineEditor : GraphView
    {
        private SkillEditorWindow skillEditorWindow;

        public SkillMachineEditor(SkillEditorWindow editorWindow, VisualElement parent, SkillLayerData skillLayerData)
        {
            Insert(0, new GridBackground());
            this.StretchToParentSize();
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            Toolbar toolbar = new Toolbar();
            Button button = new ToolbarButton(() =>
            {
                editorWindow.stateEditor = null;
                editorWindow.rootVisualElement.Remove(parent);
            });
            button.name = "btn_Back";
            button.text = "Back";
            toolbar.Add(button);
            this.Add(toolbar);
        }
    }
}
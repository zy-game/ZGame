using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.Command;

namespace ZGame.Editor.Inspector
{
    [CustomEditor(typeof(BuilderConfig))]
    public class BuilderConfigInspectorWindow : OdinEditor
    {
        private ResConfigInspectorWindow window;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (EditorGUILayout.DropdownButton(new GUIContent("Build Hotfix Package"), FocusType.Passive))
            {
                GenericSelector<PackageSeting> selector = CreateSelector();
                selector.SelectionConfirmed += enumerable => { BuildResourcePackageCommand.Executer(enumerable.ToArray()); };
                selector.ShowInPopup();
            }
        }

        private GenericSelector<PackageSeting> CreateSelector()
        {
            IEnumerable<PackageSeting> source = BuilderConfig.instance.packages;
            var colltion = source.Select(x => new GenericSelectorItem<PackageSeting>(Path.GetFileName((string)x.title), x)).ToList();
            GenericSelector<PackageSeting> selector = new GenericSelector<PackageSeting>("", false, colltion);
            selector.FlattenedTree = true;
            selector.CheckboxToggle = true;
            selector.SelectionTree.Selection.SupportsMultiSelect = true;
            selector.DrawConfirmSelectionButton = true;
            selector.SelectionTree.Config.DrawSearchToolbar = true;
            selector.SetSelection(new List<PackageSeting>());
            selector.SelectionTree.EnumerateTree().AddThumbnailIcons(true);
            selector.SelectionTree.EnumerateTree((Action<OdinMenuItem>)(x => x.Toggled = true));
            selector.SelectionTree.SortMenuItemsByName();
            return selector;
        }
    }
}
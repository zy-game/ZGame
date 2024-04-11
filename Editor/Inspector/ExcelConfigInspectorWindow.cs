using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ExcelExprot;

namespace ZGame.Editor.Inspector
{
    [CustomEditor(typeof(ExcelConfigList))]
    public class ExcelConfigInspectorWindow : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (EditorGUILayout.DropdownButton(new GUIContent("Generic"), FocusType.Passive))
            {
                if (ExcelConfigList.instance.output == null)
                {
                    EditorUtility.DisplayDialog("提示", "未选择文件导出路径", "确定");
                    return;
                }

                GenericSelector<ExcelImportOptions> selector = CreateSelector();
                selector.SelectionConfirmed += enumerable => { ExcelConfigList.instance.Generic(enumerable.ToArray()); };
                selector.ShowInPopup();
            }
        }

        private GenericSelector<ExcelImportOptions> CreateSelector()
        {
            IEnumerable<ExcelImportOptions> source = ExcelConfigList.instance.exporters;
            var colltion = source.Select(x => new GenericSelectorItem<ExcelImportOptions>(Path.GetFileName(x.path), x)).ToList();
            GenericSelector<ExcelImportOptions> selector = new GenericSelector<ExcelImportOptions>("", false, colltion);
            selector.FlattenedTree = true;
            selector.CheckboxToggle = true;
            selector.SelectionTree.Selection.SupportsMultiSelect = true;
            selector.DrawConfirmSelectionButton = true;
            selector.SelectionTree.Config.DrawSearchToolbar = true;
            selector.SetSelection(new List<ExcelImportOptions>());
            selector.SelectionTree.EnumerateTree().AddThumbnailIcons(true);
            selector.SelectionTree.EnumerateTree((Action<OdinMenuItem>)(x => x.Toggled = true));
            selector.SelectionTree.SortMenuItemsByName();
            return selector;
        }
    }
}
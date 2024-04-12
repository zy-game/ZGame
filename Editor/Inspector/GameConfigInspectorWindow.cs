using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.Command;

namespace ZGame.Editor.Inspector
{
    [CustomEditor(typeof(GameConfig))]
    public class GameConfigInspectorWindow : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (EditorGUILayout.DropdownButton(new GUIContent("Generic"), FocusType.Passive))
            {
                GenericSelector<PacketOption> selector = CreateSelector();
                selector.SelectionConfirmed += enumerable =>
                {
                    BuildGameChannelCommand.SetPlayerSetting(enumerable.FirstOrDefault(), GameConfig.instance.version);
                    BuildGameChannelCommand.Executer(enumerable.ToArray());
                };
                selector.ShowInPopup();
            }
        }

        private GenericSelector<PacketOption> CreateSelector()
        {
            IEnumerable<PacketOption> source = GameConfig.instance.channels;
            var colltion = source.Select(x => new GenericSelectorItem<PacketOption>(Path.GetFileName((string)x.title), x)).ToList();
            GenericSelector<PacketOption> selector = new GenericSelector<PacketOption>("", false, colltion);
            selector.FlattenedTree = true;
            selector.CheckboxToggle = true;
            selector.DrawConfirmSelectionButton = true;
            selector.SelectionTree.Config.DrawSearchToolbar = true;
            selector.SetSelection(new List<PacketOption>());
            selector.SelectionTree.EnumerateTree().AddThumbnailIcons(true);
            selector.SelectionTree.EnumerateTree((Action<OdinMenuItem>)(x => x.Toggled = true));
            selector.SelectionTree.SortMenuItemsByName();
            return selector;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public class SelectorAttribute : Attribute
    {
        public string ValuesGetter;

        public SelectorAttribute(string getter)
        {
            ValuesGetter = getter;
        }
    }

    public class SelectorPropertyDrawer : OdinAttributeDrawer<SelectorAttribute>
    {
        private ValueResolver<object> rawGetter;
        private Func<IEnumerable<ValueDropdownItem>> getValues;
        private Func<IEnumerable<object>> getSelection;
        private IEnumerable<object> result;

        protected override void Initialize()
        {
            this.rawGetter = ValueResolver.Get<object>(this.Property, this.Attribute.ValuesGetter);
            this.getSelection = (Func<IEnumerable<object>>)(() => this.Property.ValueEntry.WeakValues.Cast<object>());
            this.getValues = (Func<IEnumerable<ValueDropdownItem>>)(() =>
            {
                object source = this.rawGetter.GetValue();
                return source != null
                    ? (source as IEnumerable).Cast<object>().Where<object>((Func<object, bool>)(x => x != null)).Select<object, ValueDropdownItem>((Func<object, ValueDropdownItem>)(x => { return new ValueDropdownItem((string)null, x); }))
                    : (IEnumerable<ValueDropdownItem>)null;
            });
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (EditorGUILayout.DropdownButton(label, FocusType.Passive))
            {
                this.ShowSelector(new Rect(Event.current.mousePosition, Vector2.zero)).SelectionConfirmed += (Action<IEnumerable<object>>)(x => AddResult(x));
            }
        }

        private void AddResult(IEnumerable<object> query)
        {
            ICollectionResolver childResolver = this.Property.ChildResolver as ICollectionResolver;
            childResolver.QueueClear();
            foreach (object obj in query)
            {
                object[] values = new object[this.Property.ParentValues.Count];
                for (int index = 0; index < values.Length; ++index)
                    values[index] = obj;
                childResolver.QueueAdd(values);
            }
        }

        private OdinSelector<object> ShowSelector(Rect rect)
        {
            GenericSelector<object> selector = this.CreateSelector();
            rect.x = (float)(int)rect.x;
            rect.y = (float)(int)rect.y;
            rect.width = (float)(int)rect.width;
            rect.height = (float)(int)rect.height;
            selector.ShowInPopup(rect, new Vector2());
            return (OdinSelector<object>)selector;
        }

        private GenericSelector<object> CreateSelector()
        {
            IEnumerable<ValueDropdownItem> source = this.getValues() ?? Enumerable.Empty<ValueDropdownItem>();
            GenericSelector<object> selector = new GenericSelector<object>("", false, source.Select<ValueDropdownItem, GenericSelectorItem<object>>((Func<ValueDropdownItem, GenericSelectorItem<object>>)(x => new GenericSelectorItem<object>(x.Text, x.Value))));
            selector.FlattenedTree = true;
            selector.CheckboxToggle = true;
            selector.SelectionTree.Selection.SupportsMultiSelect = true;
            selector.DrawConfirmSelectionButton = true;
            selector.SelectionTree.Config.DrawSearchToolbar = true;
            IEnumerable<object> selection = Enumerable.Empty<object>();
            selection = this.getSelection().SelectMany<object, object>((Func<object, IEnumerable<object>>)(x => (x as IEnumerable).Cast<object>()));
            selector.SetSelection(selection);
            selector.SelectionTree.EnumerateTree().AddThumbnailIcons(true);
            selector.SelectionTree.EnumerateTree((Action<OdinMenuItem>)(x => x.Toggled = true));
            selector.SelectionTree.SortMenuItemsByName();
            return selector;
        }
    }
}
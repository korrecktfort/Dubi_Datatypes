using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using UnityEngine;

public class TableRowsElement : VisualElement
{
    public ScrollView ListViewScrollView => this.listView.Q<ScrollView>();

    ListView listView = new ListView(null, 20);
    /// listview entry remove workaround
    int removedIndex = -1;

    bool[] foldIns = new bool[0];

    public TableRowsElement(StyleSheet tableSheet)
    {
        base.styleSheets.Add(tableSheet);

        AddToClassList("table__rows");

        this.listView.AddToClassList("vertScroll__table");

        this.listView.makeItem = () => new TableRowsLineElement(tableSheet);
        this.listView.showBoundCollectionSize = false;
        this.listView.reorderable = true;
        this.listView.reorderMode = ListViewReorderMode.Animated;
        this.listView.showAddRemoveFooter = true;
        this.listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

        /// listview entry remove workaround
        this.listView.itemsRemoved += e =>
            this.removedIndex = this.listView.selectedIndex;

        Add(this.listView);
    }

    public void BindProperty(SerializedProperty rows)
    {
        this.listView.bindItem = (element, rowIndex) =>
        {
            if (this.removedIndex == rowIndex)
            {
                this.removedIndex = -1;
                return;
            }

            TableRowsLineElement e = element as TableRowsLineElement;            
            e.BindProperty(rows.GetArrayElementAtIndex(rowIndex));

            for (int i = 0; i < this.foldIns.Length; i++)
                if (this.foldIns[i])
                    e.ToggleFoldIn(i);
        };

        this.listView.BindProperty(rows);
    }

    public void SetButtonClickable(ListView listView, Action OnAdd)
    {
        listView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(OnAdd);
    }

    public void RemoveButtonClickable(ListView listView, Action OnRemove)
    {
        listView.Q<Button>("unity-list-view__remove-button").clickable = new Clickable(OnRemove);
    }

    public void InjectFoldIns(bool[] foldIns)
    {
        this.foldIns = foldIns;
    }

    public void ToggleFoldIn(int column, bool[] foldIns)
    {
        this.foldIns = foldIns;

        UQueryBuilder<TableRowsLineElement> elements = this.listView.Query<TableRowsLineElement>();
        elements.ForEach((element) =>
        {
            element.ToggleFoldIn(column);
        });
    }
}
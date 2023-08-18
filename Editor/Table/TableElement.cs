using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System;

public class TableElement : VisualElement
{
    string[] testTitles = new string[3] { "Entry 01", "Entry 02", "Entry 03" };
    bool[] testFolded = new bool[3];

    SerializedProperty tableProperty = null;

    VisualElement headerElement = null;
    VisualElement headerContent = null;
    VisualElement bodyElement = null;

    Button addColumn = null;

    public bool[] Folded
    {
        get
        {
            if(this.tableProperty == null)
                return this.testFolded;

            SerializedProperty foldedProperty = tableProperty.FindPropertyRelative("folded");
            bool[] folded = new bool[foldedProperty.arraySize];
            for (int i = 0; i < folded.Length; i++)            
                folded[i] = foldedProperty.GetArrayElementAtIndex(i).boolValue;
            return folded;
        }

        set
        {
            if(this.tableProperty == null)
            {
                this.testFolded = value;
                return;
            }

            SerializedProperty foldedProperty = tableProperty.FindPropertyRelative("folded");
            for (int i = 0; i < value.Length; i++)
                foldedProperty.GetArrayElementAtIndex(i).boolValue = value[i];

            foldedProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    public string[] Titles
    {
        get
        {
            if(this.tableProperty == null)
                return this.testTitles;

            SerializedProperty titlesProperty = tableProperty.FindPropertyRelative("titles");
            string[] titles = new string[titlesProperty.arraySize];
            for (int i = 0; i < titles.Length; i++)
                titles[i] = titlesProperty.GetArrayElementAtIndex(i).stringValue;
            return titles;
        }

        set
        {
            if(this.tableProperty == null)
            {
                this.testTitles = value;
                return;
            }    

            SerializedProperty titlesProperty = tableProperty.FindPropertyRelative("titles");
            titlesProperty.arraySize = value.Length;
            for (int i = 0; i < value.Length; i++)
                titlesProperty.GetArrayElementAtIndex(i).stringValue = value[i];

            titlesProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    public new class UxmlFactory : UxmlFactory<TableElement, UxmlTraits> { }

    public TableElement()
    {
        this.AddToClassList("table-element");

        /// Setup Header Element
        this.headerElement = new VisualElement() { name = "Header"};
        this.headerElement.AddToClassList("table-element__header-element");
        Add(this.headerElement);

        this.addColumn = new Button(() => this.AddContentColumn()) { text = "+", tooltip = "Add Column" };
        this.addColumn.AddToClassList("table-element__add-column-button");
        this.headerElement.Add(this.addColumn);


        /// Setup Header Content
        this.headerContent = new VisualElement() { name = "HeaderContent"};
        this.headerContent.AddToClassList("table-element__header-content");
        this.headerElement.Add(this.headerContent);
        this.headerContent.SendToBack();
        
        VisualElement headerSpacer = new VisualElement() { name = "HeaderSpacer"};
        headerSpacer.AddToClassList("table-element__header-spacer");
        this.headerElement.Add(headerSpacer);
        headerSpacer.SendToBack();

        /// Setup Body Element
        this.bodyElement = new VisualElement() { name = "Body"};
        this.bodyElement.AddToClassList("table-element__body-element");
        Add(this.bodyElement);

        SetupTitles();
        this.bodyElement.Add(new MultiDimensionalElement());
    }

    void SetupTitles()
    {
        this.headerContent.Clear();

        for (int i = 0; i < this.Titles.Length; i++)
        {
            int index = i;

            Cell cell = new Cell() { ColumnIndex = index };

            TextField textField = new TextField() { value = this.Titles[cell.ColumnIndex] };
            textField.RegisterValueChangedCallback((e) =>
            {
                string[] titles = this.Titles;
                titles[cell.ColumnIndex] = e.newValue;
                Titles = titles;
            });

            cell.InjectContextMenuItems(
                new ContextMenuItem("Delete Column", () =>
                {                    
                    List<string> list = Titles.ToList();
                    list.RemoveAt(cell.ColumnIndex);
                    Titles = list.ToArray();
                    cell.RemoveFromHierarchy();

                    RemoveContentColumn(cell.ColumnIndex);
                }
                ));

            cell.Add(textField);    
            
            this.headerContent.Add(cell);
        }
    }

    private void AddContentColumn()
    {
        List<string> list = Titles.ToList();
        list.Add("Entry " + (list.Count + 1).ToString("00"));
        Titles = list.ToArray();

        this.Query<MultiDimensionalElement>().ForEach((e) => e.AddColumn());
        SetupTitles();

        UpdateColumnIndices();
    }

    private void RemoveContentColumn(int index)
    {
        this.Query<MultiDimensionalElement>().ForEach((e) => e.RemoveColumn(index));
        UpdateColumnIndices();
    }

    public void Inject(SerializedProperty tableProperty)
    {
        this.tableProperty = tableProperty;

        /// Setup Header Element
        this.headerContent.Clear();
        SetupTitles();

        /// Setup Body Element
        this.bodyElement.Clear();
        this.bodyElement.Add(new MultiDimensionalElement(tableProperty.FindPropertyRelative("data"), this.Titles.Length));
    }

    void UpdateColumnIndices()
    {
        this.headerContent.Query<Cell>().ForEach(
        cell =>
            {
                cell.ColumnIndex = cell.parent.IndexOf(cell);
            });
    }
}

public class MultiDimensionalElement : ListView
{
    SerializedProperty rowsProperty = null;

    public new class UxmlFactory : UxmlFactory<MultiDimensionalElement, UxmlTraits> { }

    int columns = 0;

    public int Columns { get => columns; set => columns = value; }

    public MultiDimensionalElement()
    {
        this.AddToClassList("multi-dimensional-element");

        base.fixedItemHeight = EditorGUIUtility.singleLineHeight;
        base.makeItem = () => new SingleDimensionalElement();
        base.bindItem = (e, i) => (e as SingleDimensionalElement).Inject(PrepareRowArrayLength(this.rowsProperty.GetArrayElementAtIndex(i), this.columns));
        base.unbindItem = (e, i) => (e as SingleDimensionalElement).Clear();
        base.showBoundCollectionSize = false;
        base.reorderable = true;
        base.reorderMode = ListViewReorderMode.Animated;
        base.showAddRemoveFooter = true;
        base.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
    }  

    public MultiDimensionalElement(SerializedProperty dataProperty, int columns) : this()
    {
        this.columns = columns;
        this.rowsProperty = dataProperty.FindPropertyRelative("rows");
        this.BindProperty(this.rowsProperty);

        RegisterCallback<MouseDownEvent>((evt) => 
        {
            switch(evt.button)
            {
                case 1:
                    if (evt.target is Cell cell)
                        cell.DisplayContextMenu();
                    break;
            }
        });
    }

    public void AddColumn()
    {
        if (this.rowsProperty == null)
            return;        

        this.columns++;
        
        this.Query<SingleDimensionalElement>().ForEach(e => e.AddColumn());
        
        this.RefreshItems();
    }

    public void RemoveColumn(int index)
    {
        if(this.rowsProperty == null)
            return;

        this.columns--;

        this.Query<SingleDimensionalElement>().ForEach(e => e.RemoveColumn(index));

        this.RefreshItems();
    }


    /// <summary>
    /// Set the array size of the row according to the length of the columns
    /// </summary>
    public SerializedProperty PrepareRowArrayLength(SerializedProperty rowProperty, int length)
    {
        SerializedProperty array = rowProperty.FindPropertyRelative("array");
        array.arraySize = length;
        array.serializedObject.ApplyModifiedProperties();
        return rowProperty;
    }
}

public class SingleDimensionalElement : VisualElement
{    
    public new class UxmlFactory : UxmlFactory<SingleDimensionalElement, UxmlTraits> { }

    public bool Folded
    {
        get => ClassListContains("single-dimensional-element--folded");

        set
        {
            if (value)
                AddToClassList("single-dimensional-element--folded");
            else
                RemoveFromClassList("single-dimensional-element--folded");
        }
    }

    SerializedProperty arrayProperty = null;

    public SingleDimensionalElement()
    {
        this.AddToClassList("single-dimensional-element");
    }

    public void Inject(SerializedProperty rowProperty)
    {
        SerializedProperty arrayProperty = rowProperty.FindPropertyRelative("array");
        this.arrayProperty = arrayProperty;

        for (int i = 0; i < arrayProperty.arraySize; i++)
        {
            int index = i;
            Cell cell = new Cell() { ColumnIndex = index};
            cell.Inject(arrayProperty.GetArrayElementAtIndex(i));
            Add(cell);
        }
    }

    public void AddColumn()
    {
        this.arrayProperty.arraySize++;
        this.arrayProperty.serializedObject.ApplyModifiedProperties();

        UpdateCellIndices();
    }

    public void RemoveColumn(int index)
    {
        this.arrayProperty.DeleteArrayElementAtIndex(index);
        this.arrayProperty.serializedObject.ApplyModifiedProperties();

        UpdateCellIndices();
    }

    public void UpdateCellIndices()
    {
        this.Query<Cell>().ForEach(
        cell =>
        {
            cell.ColumnIndex = cell.parent.IndexOf(cell);
        });
    }
}

public class Cell : VisualElement
{   
    public new class UxmlFactory : UxmlFactory<Cell, UxmlTraits> { }

    public bool Folded
    {
        get => ClassListContains("cell--folded");

        set
        {
            if (value)
                AddToClassList("cell--folded");
            else
                RemoveFromClassList("cell--folded");
        }
    }

    public int ColumnIndex { get => columnIndex; set => columnIndex = value; }

    
    int columnIndex = -1;
    List<ContextMenuItem> contextMenuItems = new List<ContextMenuItem>();

    public Cell()
    {
        this.AddToClassList("cell");

        RegisterCallback<MouseDownEvent>((evt) =>
        {
            switch (evt.button)
            {
                case 1:
                    DisplayContextMenu();
                    break;
            }
        });
    }

    public void Inject(SerializedProperty cellProperty)
    {        
        PropertyField propertyField = new PropertyField() { label = ""};
        propertyField.BindProperty(cellProperty);
        Add(propertyField);
    }

    public void InjectContextMenuItems(params ContextMenuItem[] items)
    {
        this.contextMenuItems.AddRange(items);

        
    }

    public void DisplayContextMenu()
    {
        GenericMenu menu = new GenericMenu();

        foreach (var item in this.contextMenuItems)
        {
            menu.AddItem(new GUIContent(item.Name), false, () => item.Action());
        }

        menu.ShowAsContext();
    }
}

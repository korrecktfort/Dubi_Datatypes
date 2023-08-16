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

        this.addColumn = new Button(() => this.AddColumn()) { text = "Add Column" };
        this.addColumn.AddToClassList("table-element__add-column-button");
        this.headerElement.Add(this.addColumn);

        /// Setup Header Content
        this.headerContent = new VisualElement() { name = "HeaderContent"};
        this.headerContent.AddToClassList("table-element__header-content");
        this.headerElement.Add(this.headerContent);

        /// Setup Body Element
        this.bodyElement = new VisualElement() { name = "Body"};
        this.bodyElement.AddToClassList("table-element__body-element");
        Add(this.bodyElement);

        this.bodyElement.Add(new MultiDimensionalElement());
    }

    private void AddColumn()
    {
        this.Query<MultiDimensionalElement>().ForEach((e) => e.AddColumn());        
    }

    public void Inject(SerializedProperty tableProperty)
    {
        /// Setup Header Element
        this.headerContent.Clear();

        /// Setup Body Element
        this.bodyElement.Clear();
        this.tableProperty = tableProperty;
        this.bodyElement.Add(new MultiDimensionalElement(tableProperty.FindPropertyRelative("data")));
    }  
}

public class MultiDimensionalElement : ListView
{
    SerializedProperty rowsProperty = null;

    public new class UxmlFactory : UxmlFactory<MultiDimensionalElement, UxmlTraits> { }

    public MultiDimensionalElement()
    {
        this.AddToClassList("multi-dimensional-element");

        base.fixedItemHeight = EditorGUIUtility.singleLineHeight;
        base.makeItem = () => new SingleDimensionalElement();
        base.bindItem = (e, i) => (e as SingleDimensionalElement).Inject(this.rowsProperty.GetArrayElementAtIndex(i));
        base.unbindItem = (e, i) => (e as SingleDimensionalElement).Clear();
        base.showBoundCollectionSize = false;
        base.reorderable = true;
        base.reorderMode = ListViewReorderMode.Animated;
        base.showAddRemoveFooter = true;
        base.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
    }  

    public MultiDimensionalElement(SerializedProperty dataProperty) : this()
    {
        this.rowsProperty = dataProperty.FindPropertyRelative("rows");
        this.BindProperty(this.rowsProperty);
    }

    public void AddColumn()
    {
        if (this.rowsProperty == null)
            return;        

        for (int i = 0; i < this.rowsProperty.arraySize; i++)
        {
            SerializedProperty row = this.rowsProperty.GetArrayElementAtIndex(i);
            SerializedProperty rowArray = row.FindPropertyRelative("array");
            rowArray.arraySize++;
        }

        this.rowsProperty.serializedObject.ApplyModifiedProperties();
        this.RefreshItems();
    }

    public void RemoveColumn(int index)
    {
        if(this.rowsProperty == null)
            return;

        for (int i = 0; i < this.rowsProperty.arraySize; i++)
        {
            SerializedProperty row = this.rowsProperty.GetArrayElementAtIndex(i);
            SerializedProperty rowArray = row.FindPropertyRelative("array");
            rowArray.DeleteArrayElementAtIndex(index);
        }

        this.rowsProperty.serializedObject.ApplyModifiedProperties();
        this.RefreshItems();
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

    public SingleDimensionalElement()
    {
        this.AddToClassList("single-dimensional-element");
    }

    public void Inject(SerializedProperty rowProperty)
    {
        SerializedProperty cellsProperty = rowProperty.FindPropertyRelative("array");
        for (int i = 0; i < cellsProperty.arraySize; i++)
        {
            Cell cell = new Cell();
            cell.Inject(cellsProperty.GetArrayElementAtIndex(i));
            Add(cell);
        }
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

    public Cell()
    {
        this.AddToClassList("cell");
    }

    public void Inject(SerializedProperty cellProperty)
    {        
        PropertyField propertyField = new PropertyField() { label = ""};
        propertyField.BindProperty(cellProperty);
        Add(propertyField);
    }
}

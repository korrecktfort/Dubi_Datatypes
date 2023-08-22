using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.IO;

public class TableElement : VisualElement, OptionsPropertyFieldBind
{
    string[] testTitles = new string[3] { "Entry 01", "Entry 02", "Entry 03" };
    bool[] testFolded = new bool[3];
       
    VisualElement headerElement = null;
    VisualElement headerContent = null;
    VisualElement bodyElement = null;
    VisualElement optionsIcon = null;

    Button addColumn = null;
    SerializedObject serializedTable = null;

    public bool[] Folded
    {
        get
        {
            if(this.serializedTable == null)
                return this.testFolded;

            SerializedProperty foldedProperty = serializedTable.FindProperty("folded");
            bool[] folded = new bool[foldedProperty.arraySize];
            for (int i = 0; i < folded.Length; i++)            
                folded[i] = foldedProperty.GetArrayElementAtIndex(i).boolValue;
            return folded;
        }

        set
        {
            if(this.serializedTable == null)
            {
                this.testFolded = value;
                return;
            }

            SerializedProperty foldedProperty = serializedTable.FindProperty("folded");
            for (int i = 0; i < value.Length; i++)
                foldedProperty.GetArrayElementAtIndex(i).boolValue = value[i];

            foldedProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    public string[] Titles
    {
        get
        {
            if(this.serializedTable == null)
                return this.testTitles;

            SerializedProperty titlesProperty = serializedTable.FindProperty("titles");
            string[] titles = new string[titlesProperty.arraySize];
            for (int i = 0; i < titles.Length; i++)
                titles[i] = titlesProperty.GetArrayElementAtIndex(i).stringValue;
            return titles;
        }

        set
        {
            if(this.serializedTable == null)
            {
                this.testTitles = value;
                return;
            }    

            SerializedProperty titlesProperty = serializedTable.FindProperty("titles");
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
        this.headerElement = new VisualElement() { name = "Header" };
        this.headerElement.AddToClassList("table-element__header-element");
        Add(this.headerElement);

        this.addColumn = new Button(() => this.AddContentColumn()) { text = "+", tooltip = "Add Column" };
        this.addColumn.AddToClassList("table-element__add-column-button");
        this.headerElement.Add(this.addColumn);

        /// Setup Header Content
        this.headerContent = new VisualElement() { name = "HeaderContent" };
        this.headerContent.AddToClassList("table-element__header-content");
        this.headerElement.Add(this.headerContent);
        this.headerContent.SendToBack();

        VisualElement headerSpacer = new VisualElement() { name = "HeaderSpacer" };
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

    #region Titles

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

                    RemoveContentColumn(cell.ColumnIndex);
                }
                ));

            cell.Add(textField);    
            
            this.headerContent.Add(cell);
        }
    }

    #endregion

    #region Columns
    private void AddContentColumn()
    {
        List<string> list = Titles.ToList();
        list.Add("Entry " + (list.Count + 1).ToString("00"));
        Titles = list.ToArray();

        this.Query<MultiDimensionalElement>().ForEach((e) => e.AddColumn());
        SetupTitles();

        UpdateCells();
    }

    private void RemoveContentColumn(int index)
    {
        this.Query<Cell>().ForEach((c) =>
        {
            if (c.ColumnIndex != index)
                return;

            c.RemoveFromHierarchy();
        });

        this.Query<MultiDimensionalElement>().ForEach((e) => e.RemoveColumn(index));

        UpdateCells();
    }

    void UpdateCells()
    {
        this.Query<Cell>().ForEach(cell =>
        {
            cell.SetColumnIndex();
        });
    }

    public void SetColumnMinWidth(int column)
    {
        int minWidth = 0;

        this.Query<Cell>().ForEach(cell =>
        {
            if (cell.ColumnIndex == column)
                minWidth = Mathf.Max(minWidth, (int)cell.layout.width);
        });

        this.Query<Cell>().ForEach(cell =>
        {
            if (cell.ColumnIndex == column)
                cell.ColumnWidth = minWidth;
        });
    }    
    #endregion

    #region Injection
    public void BindSerializedObject(SerializedObject serializedObject)
    {
        this.serializedTable = serializedObject;

        this.headerContent.Clear();
        this.bodyElement.Clear();

        this.style.display = new StyleEnum<DisplayStyle>(serializedObject != null ? DisplayStyle.Flex : DisplayStyle.None);

        if (serializedObject == null)
            return;

        /// Setup Header Element
        SetupTitles();

        /// Setup Body Element
        this.bodyElement.Add(new MultiDimensionalElement(serializedObject.FindProperty("data"), this.Titles.Length));
    }
    #endregion    

}

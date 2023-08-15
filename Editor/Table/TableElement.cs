using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TableElement : VisualElement
{
    public VisualElement ToolBar => this.toolbar;

    VisualElement toolbar = new VisualElement();
    TableHeaderRowElement headerRowElement = null;
    TableRowsElement rowsElement = null;
    bool[] foldIns = new bool[0];

    public TableElement()
    {
        StyleSheet tableSheet = Resources.Load<StyleSheet>("TableUSS");
        base.styleSheets.Add(tableSheet);
        AddToClassList("table");

        this.headerRowElement = new TableHeaderRowElement(tableSheet, OnColumnClicked);
        this.rowsElement = new TableRowsElement(tableSheet);
        this.toolbar.AddToClassList("table__toolbar");

        Add(this.toolbar);
        Add(this.headerRowElement);
        Add(this.rowsElement);

        this.rowsElement.ListViewScrollView.horizontalScroller.valueChanged += OnHorizontalScrollerChanged;

    } 

    public void BindProperty(SerializedProperty tableProperty)
    {
        SerializedProperty titles = tableProperty.FindPropertyRelative("titles");
        this.headerRowElement.BindProperty(titles);

        SerializedProperty rows = tableProperty.FindPropertyRelative("data.rows");
        this.rowsElement.BindProperty(rows);

        this.foldIns = new bool[titles.arraySize];
        this.rowsElement.InjectFoldIns(this.foldIns);
    }

    void OnColumnClicked(int column)
    {
        this.foldIns[column] = !this.foldIns[column];
        this.headerRowElement.ToggleFoldIn(column);
        this.rowsElement.ToggleFoldIn(column, this.foldIns);
    }

    void OnHorizontalScrollerChanged(float value)
    {
        this.headerRowElement.SetXOffset(Mathf.Min(-value, 0.0f));
    }
}
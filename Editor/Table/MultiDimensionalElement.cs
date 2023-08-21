using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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

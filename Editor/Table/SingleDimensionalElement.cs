using UnityEditor;
using UnityEngine.UIElements;

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
    }

    public void RemoveColumn(int index)
    {
        this.arrayProperty.DeleteArrayElementAtIndex(index);
        this.arrayProperty.serializedObject.ApplyModifiedProperties();
    }
}

using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class TableRowsLineElement : VisualElement
{
    Button optionButton = new Button();

    VisualElement fieldsContainer = new VisualElement();
    PropertyField[] fields = new PropertyField[0];

    public TableRowsLineElement(StyleSheet styleSheet)
    {
        this.styleSheets.Add(styleSheet);

        Add(this.optionButton);
        this.optionButton.AddToClassList("entry-line__button");

        AddToClassList("entry-line");
        Add(this.fieldsContainer);
        this.fieldsContainer.AddToClassList("entry-line__fieldsContainer");
    }

    public void BindProperty(SerializedProperty row)
    {
        SerializedProperty entries = row.FindPropertyRelative("array");

        this.fieldsContainer.Clear();
        this.fields = new PropertyField[entries.arraySize];        

        for (int i = 0; i < entries.arraySize; i++)
        {
            PropertyField field = new PropertyField();
            field.AddToClassList("entry-line__field");
            this.fieldsContainer.Add(field);
            field.BindProperty(entries.GetArrayElementAtIndex(i));
            this.fields[i] = field;
        }
    }

    public void ToggleFoldIn(int column)
    {
        this.fields[column].ToggleInClassList("fold-in");
    }
}

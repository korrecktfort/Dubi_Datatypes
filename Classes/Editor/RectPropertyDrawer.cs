using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Rect))]
public class RectPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position.height = EditorGUIUtility.singleLineHeight;
        float space = 5.0f;
        float width = (position.width - space) * 0.5f;
        float labelWidth = 15.0f;
        float cachedLabelWidth = EditorGUIUtility.labelWidth;   
        float x = position.x;
        EditorGUIUtility.labelWidth = labelWidth;
        position.width = width;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("x"), new GUIContent("X"));
        position.x += width + space;        
        EditorGUI.PropertyField(position, property.FindPropertyRelative("y"), new GUIContent("Y"));
        position.x = x;
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("width"), new GUIContent("W"));
        position.x += width + space;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("height"), new GUIContent("H"));

        EditorGUI.EndProperty();        
        EditorGUIUtility.labelWidth = cachedLabelWidth;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2.0f + 2.0f * EditorGUIUtility.standardVerticalSpacing;
    }
}

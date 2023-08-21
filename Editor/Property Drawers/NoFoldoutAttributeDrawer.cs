using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NoFoldoutAttribute))]
public class NoFoldoutAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.indentLevel = property.depth;

        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(position, property.displayName);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;


        foreach(string s in PropertyPaths(property))
        {
            SerializedProperty p = property.serializedObject.FindProperty(s);
            if (p != null)
            {
                EditorGUI.PropertyField(position, p, true);
                position.y += EditorGUI.GetPropertyHeight(p, true) + EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        List<string> paths = PropertyPaths(property);
        foreach (string s in paths)
        {
            SerializedProperty p = property.serializedObject.FindProperty(s);
            if (p != null)
            {
                height += EditorGUI.GetPropertyHeight(p, true) + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        return height;
    }

    List<string> PropertyPaths(SerializedProperty property)
    {
        int depth = property.depth;

        List<string> paths = new List<string>();
        SerializedProperty endProperty = property.GetEndProperty();
        while (property.NextVisible(true) && !SerializedProperty.EqualContents(property, endProperty))
        {
            if(property.depth > depth + 1)            
                continue;            

            paths.Add(property.propertyPath);
        }
        return paths;
    }
}

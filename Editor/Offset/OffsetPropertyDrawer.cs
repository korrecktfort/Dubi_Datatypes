using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OffsetAttribute))]
public class OffsetPropertyDrawer : PropertyDrawer
{
    /// Icons
    string minus = "d_Toolbar Minus";
    string plus = "d_Toolbar Plus";

    float unit = EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(property.propertyType != SerializedPropertyType.Vector2 && property.propertyType != SerializedPropertyType.Vector3)
        {
            EditorGUI.HelpBox(position, "Offset attribute can only be used on Vector2 or Vector3", MessageType.Warning);
            base.OnGUI(position, property, label);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);
        position.x += EditorGUI.indentLevel * 15.0f;
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(position, property.displayName + ":");
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        Vector2 origin = position.position;
        float unit = this.unit;
        Vector2 size = Vector2.one * unit;
        Rect reset = new Rect(Vector2.zero + origin, Vector2.one * unit);
        
        Rect xfield = new Rect(Vector2.right * unit * 2.0f + origin, new Vector2(2.0f * unit, unit));
        Rect xMinus = new Rect(Vector2.right * unit + origin, size);
        Rect xPlus = new Rect(Vector2.right * unit * 4.0f + origin, size);

        Rect yMinus = new Rect(Vector2.up * unit + origin, size);
        Rect yfield = new Rect(Vector2.up * unit * 2.0f + new Vector2(-0.5f * unit, 0.5f * unit) + origin, new Vector2(2.0f * unit, unit));
        Rect yPlus = new Rect(Vector2.up * unit * 4.0f + origin, size);


        position.width = unit * 5.0f;
        position.height = unit * 5.0f;

        SerializedProperty x = property.FindPropertyRelative("x");
        SerializedProperty y = property.FindPropertyRelative("y");

        Button(reset, "R", () => 
        { 
            x.floatValue = 0.0f; 
            y.floatValue = 0.0f; 
            
            if(property.propertyType == SerializedPropertyType.Vector3)
                property.FindPropertyRelative("z").floatValue = 0.0f;
            
            property.serializedObject.ApplyModifiedProperties(); 
        }, true);

        Vector2 iconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(Vector2.one * unit * 0.75f);

        Button(xMinus, minus, () => { x.floatValue--; x.serializedObject.ApplyModifiedProperties(); });
        Field(xfield, x);
        Button(xPlus, plus, () => { x.floatValue++; x.serializedObject.ApplyModifiedProperties(); });
        
        Button(yMinus, minus, () => { y.floatValue--; y.serializedObject.ApplyModifiedProperties(); });
        Field(yfield, y, 90.0f);
        Button(yPlus, plus, () => { y.floatValue++; y.serializedObject.ApplyModifiedProperties(); });

        if(property.propertyType == SerializedPropertyType.Vector3)
        {
            Vector2 pivot = origin + new Vector2(3.0f * unit, 2.5f * unit);

            Rect zMinus = new Rect(pivot - Vector2.right * unit * 2.0f, size);
            Rect zfield = new Rect(pivot - Vector2.right * unit, new Vector2(2.0f * unit, unit));
            Rect zPlus = new Rect(pivot + Vector2.right * unit, size);

            GUIUtility.RotateAroundPivot(45.0f, pivot);

            Button(zMinus, minus, () => { property.FindPropertyRelative("z").floatValue--; property.serializedObject.ApplyModifiedProperties(); });
            Field(zfield, property.FindPropertyRelative("z"));
            Button(zPlus, plus, () => { property.FindPropertyRelative("z").floatValue++; property.serializedObject.ApplyModifiedProperties(); });

            GUIUtility.RotateAroundPivot(-45.0f, pivot);
        }

        EditorGUIUtility.SetIconSize(iconSize);
        EditorGUI.EndProperty();
    }

    void Field(Rect position, SerializedProperty property, float angle = 0.0f, Vector2 pivot = default)
    {

        if (angle != 0.0f)
            GUIUtility.RotateAroundPivot(angle, position.center);


        EditorGUI.BeginChangeCheck();
        float value = EditorGUI.FloatField(position, GUIContent.none, property.floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            property.floatValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

        if (angle != 0.0f)
            GUIUtility.RotateAroundPivot(-angle, position.center);
    }

    void Button(Rect position, string icon, Action OnClick, bool useIconString = false)
    {
        position.width = EditorGUIUtility.singleLineHeight;

        if (useIconString)
        {
            if (GUI.Button(position, icon))
            {
                OnClick?.Invoke();
            }
        }
        else
        {
            if (GUI.Button(position, EditorGUIUtility.IconContent(icon)))
            {
                OnClick?.Invoke();
            }
        }     
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return this.unit * 5.0f + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }
}

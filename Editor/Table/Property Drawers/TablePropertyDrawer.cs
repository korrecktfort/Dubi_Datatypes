using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(Table<>), true)]
public class TablePropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        if (property.objectReferenceValue == null)
        {
            property.objectReferenceValue = ScriptableObject.CreateInstance(base.fieldInfo.FieldType);
            property.serializedObject.ApplyModifiedProperties();
        }

        VisualElement SetupTableElement(Object toSerialize)
        {
            if (toSerialize == null)
                return new VisualElement();

            VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("TableUXML");

            VisualElement root = new VisualElement();
            visualTreeAsset.CloneTree(root);

            root.Q<TableElement>().Inject(new SerializedObject(toSerialize));       
            
            return root;
        }

        VisualElement root = new VisualElement();
        //root.BindProperty(property);
        //root.style.paddingBottom = new StyleLength(-EditorGUIUtility.singleLineHeight);
        
        PropertyField propertyField = new PropertyField(property);

        VisualElement tableContainer = new VisualElement();
        
        VisualElement table = SetupTableElement(property.objectReferenceValue);
        table.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

        root.Add(propertyField);
        root.Add(tableContainer);

        propertyField.RegisterValueChangeCallback(evt =>
        {
            tableContainer.Clear();

            if(evt.changedProperty.objectReferenceValue == null)
                return;

            table = SetupTableElement(evt.changedProperty.objectReferenceValue);   

            tableContainer.Add(table);
        });
        
        propertyField.BindProperty(property);

        return root;
    }
}

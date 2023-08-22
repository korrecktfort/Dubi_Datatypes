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
        VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("TableUXML");
        VisualElement root = tree.CloneTree();

        root.Q<OptionsPropertyField>().BindProperty(property, base.fieldInfo.FieldType);
        root.Q<OptionsPropertyField>().SetupSerializedObjectBind(root.Q<TableElement>() as OptionsPropertyFieldBind);

        return root;
    }
}

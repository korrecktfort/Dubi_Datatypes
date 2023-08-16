using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Table<>))]
public class TablePropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("TableUXML");

        VisualElement root = new VisualElement();
        visualTreeAsset.CloneTree(root);

        root.Q<TableElement>().Inject(property);

        return root;
    }
}

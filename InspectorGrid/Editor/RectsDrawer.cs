using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Rects))]
public class RectsDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>("RectsDrawerUxml");               

        VisualElement root = new VisualElement();
        root.AddToClassList("rects-drawer-element");
        treeAsset.CloneTree(root);

        return root;
    }
}

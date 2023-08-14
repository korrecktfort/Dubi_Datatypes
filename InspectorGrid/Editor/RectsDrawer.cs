using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
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
        SerializedProperty rects = property.FindPropertyRelative("rects");
        root.Q<RectsDrawerElement>().BindProperty(rects);
        root.Q<ListView>().BindProperty(rects);
        root.Q<RectsDrawerElement>().InjectListView(root.Q<ListView>());
        root.Q<RectsDrawerElement>().InjectLabel(root.Q<Label>());
        return root;
    }
}

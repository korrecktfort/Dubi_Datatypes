using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Rects), true)]
public class RectsPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>("RectsDrawerUxml");               
        VisualElement root = treeAsset.CloneTree();

        root.AddToClassList("rects-drawer-element");
        root.Q<RectsDrawerElement>().InjectListView(root.Q<ListView>());
        root.Q<RectsDrawerElement>().InjectMouseCoordinateLabel(root.Q<Label>("MouseCoordinateLabel"));

        /// Additional special binding setup for Non-OpertionsPropertyFieldBind Elements
        void AdditionalBinding(SerializedObject serializedObject)
        {
            SerializedProperty rects = serializedObject?.FindProperty("rects");

            if (rects != null)
                root.Q<ListView>().BindProperty(rects);
            else
                root.Q<ListView>().Unbind();
        }

        root.Q<OptionsPropertyField>().SetupSerializedObjectBind(AdditionalBinding);

        root.Q<OptionsPropertyField>().BindProperty(property, base.fieldInfo.FieldType);
        root.Q<OptionsPropertyField>().SetupSerializedObjectBind(root.Q<RectsDrawerElement>() as OptionsPropertyFieldBind);

        return root;
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

public class Cell : VisualElement
{   
    public new class UxmlFactory : UxmlFactory<Cell, UxmlTraits> { }

    public bool Folded
    {
        get => ClassListContains("cell--folded");

        set
        {
            if (value)
                AddToClassList("cell--folded");
            else
                RemoveFromClassList("cell--folded");
        }
    }

    public int ColumnIndex { get => columnIndex; set => columnIndex = value; }

    public int ColumnWidth
    {
        get => columnWidth;
        set
        {    
            this.columnWidth = value;
            base.style.minWidth = value;
        }
    }

    int columnIndex = -1;    
    int columnWidth = 0;
    List<ContextMenuItem> contextMenuItems = new List<ContextMenuItem>();

    public Cell()
    {
        this.AddToClassList("cell");

        RegisterCallback<MouseDownEvent>((evt) =>
        {
            switch (evt.button)
            {
                case 1:
                    DisplayContextMenu();
                    break;
            }
        });
              
        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);   
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    public void OnAttachToPanel(AttachToPanelEvent evt)
    {
        SetColumnIndex();
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnDetachFromPanel(DetachFromPanelEvent evt)
    {
       UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    public void OnGeometryChanged(GeometryChangedEvent evt)
    {
        QueryForParent<TableElement>()?.SetColumnMinWidth(this.columnIndex);
    }

    public void Inject(SerializedProperty cellProperty)
    {        
        PropertyField propertyField = new PropertyField() { label = ""};
        propertyField.BindProperty(cellProperty);
        Add(propertyField);
    }

    public void InjectContextMenuItems(params ContextMenuItem[] items)
    {
        this.contextMenuItems.AddRange(items);
    }

    public void DisplayContextMenu()
    {
        GenericMenu menu = new GenericMenu();

        foreach (var item in this.contextMenuItems)
        {
            menu.AddItem(new GUIContent(item.Name), false, () => item.Action());
        }

        menu.ShowAsContext();
    }

    public void SetColumnIndex()
    {
        this.columnIndex = this.parent.IndexOf(this);
    }

    T QueryForParent<T>() where T : VisualElement
    {
        VisualElement parent = this.parent;
        int steps = 0;

        do
        {
            if (parent is T)
                return parent as T;

            parent = parent.parent;
            steps++;
        } while (parent != null && steps <= 1000);

        if (steps > 1000)
            throw new Exception("Could not find parent of type " + typeof(T).Name + " within 1000 steps.");

        return null;
    }
}

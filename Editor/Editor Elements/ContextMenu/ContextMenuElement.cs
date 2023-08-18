using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ContextMenuElement : VisualElement
{
    public ContextMenuElement(List<ContextMenuItem> contextMenuItems)
    {
        base.styleSheets.Add(Resources.Load<StyleSheet>("ContextMenuUSS"));
        AddToClassList("context-menu");

        foreach(var contextMenuItem in contextMenuItems)
        {
            Button button = new Button();
            button.AddToClassList("context-menu-item-button");
            button.text = contextMenuItem.Name;
            button.clicked += contextMenuItem.Action;
            button.clicked += RemoveFromHierarchy;
            this.Add(button);
        }
    }
}

public class ContextMenuItem
{
    public string Name => this.name;

    public Action Action => this.action;

    public ContextMenuItem(string name, Action action)
    {
        this.name = name;
        this.action = action;
    }

    string name = "";
    Action action = null;
}

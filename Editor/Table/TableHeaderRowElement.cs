using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TableHeaderRowElement : VisualElement
{
    VisualElement spaceToLeft = new VisualElement();
    VisualElement buttonsViewport = new VisualElement(); 
    VisualElement buttonsContainer = new VisualElement();
    
    Button[] columnButtons = new Button[0];
    Action<int> OnColumnClicked;

    public TableHeaderRowElement(StyleSheet tableSheet, Action<int> onColumnClicked)
    {
        base.styleSheets.Add(tableSheet);
        AddToClassList("header");

        this.spaceToLeft.AddToClassList("header__spaceToLeft");

        Add(this.buttonsViewport);
        this.buttonsViewport.Add(this.spaceToLeft);
        this.buttonsViewport.Add(this.buttonsContainer);

        this.buttonsViewport.AddToClassList("header__viewport");
        this.buttonsContainer.AddToClassList("header__viewport__container");

        this.OnColumnClicked = onColumnClicked;
    }

    public void BindProperty(SerializedProperty array)
    {
        this.columnButtons = new Button[array.arraySize];

        for (int i = 0; i < array.arraySize; i++)
        {
            Button button = new Button();
            button.AddToClassList("header__viewport__container__columnButton");
            button.text = array.GetArrayElementAtIndex(i).stringValue;
            int index = i;
            button.clickable = new Clickable(() => this.OnColumnClicked.Invoke(index));
            this.columnButtons[i] = button;

            this.buttonsContainer.Add(button);
        }
    }

    public void SetXOffset(float value)
    {
        Length y = this.buttonsContainer.style.translate.value.y;
        this.buttonsContainer.style.translate = new StyleTranslate(new Translate(new Length(value), y));
    }

    public void ToggleFoldIn(int column)
    {
        this.columnButtons[column].ToggleInClassList("fold-in");
    }
}

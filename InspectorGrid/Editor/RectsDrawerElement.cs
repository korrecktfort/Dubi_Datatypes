using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class RectsDrawerElement : InspectorGridElement
{
    enum ToolState
    {
        Dragging,
        Drawing,
        Resizing,
        None,
    }


    // Rect[] localRectsArray = new Rect[1] {new Rect(0, 0, 20, 100)};
    Rect[] localRectsArray = new Rect[0]; // {new Rect(0, 0, 20, 100)};

    Rect[] Rects
    {
        get
        {
            if(this.rectsProperty == null)
                return this.localRectsArray;

            List<Rect> list = new List<Rect>();
            for (int i = 0; i < this.rectsProperty.arraySize; i++)
            {
                list.Add(this.rectsProperty.GetArrayElementAtIndex(i).rectValue);
            }
            return list.ToArray();
        }

        set
        {
            if(this.rectsProperty == null)
            {
                this.localRectsArray = value;
                return;
            }   

            this.rectsProperty.arraySize = value.Length;
            for (int i = 0; i < value.Length; i++)
            {
                this.rectsProperty.GetArrayElementAtIndex(i).rectValue = value[i];
            }
            this.rectsProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    bool DisplayFocussed
    {
        get
        {
            return ClassListContains("rects-drawer-element--focus");
        }

        set
        {
            if(value)
                AddToClassList("rects-drawer-element--focus");
            else
                RemoveFromClassList("rects-drawer-element--focus");
        }
    }

    SerializedProperty rectsProperty = null;
    ToolState toolState = ToolState.None;
    Rect manipulationRect = default;
    Color selectedColor = Color.red;
    Color rectColor = Color.cyan;
    Color manipulationRectColor = Color.yellow;
    int selectedRectIndex = -1;
    Vector2 dragOffset = Vector2.zero;

    public Color SelectedColor { get => selectedColor; set => selectedColor = value; }
    public Color RectColor { get => rectColor; set => rectColor = value; }
    public Color ManipulationRectColor { get => manipulationRectColor; set => manipulationRectColor = value; }

    public new class UxmlFactory : UxmlFactory<RectsDrawerElement, UxmlTraits> { }

    public new class UxmlTraits : InspectorGridElement.UxmlTraits
    {
        UxmlColorAttributeDescription selectedRectColor = new UxmlColorAttributeDescription { name = "selected-color", defaultValue = Color.red };

        UxmlColorAttributeDescription rectColor = new UxmlColorAttributeDescription { name = "rect-color", defaultValue = Color.cyan };

        UxmlColorAttributeDescription manipulationRectColor = new UxmlColorAttributeDescription { name = "manipulation-rect-color", defaultValue = Color.yellow };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            RectsDrawerElement element = (RectsDrawerElement)ve;
            element.SelectedColor = selectedRectColor.GetValueFromBag(bag, cc);
            element.RectColor = rectColor.GetValueFromBag(bag, cc);
            element.ManipulationRectColor = manipulationRectColor.GetValueFromBag(bag, cc);
        }
    }

    public RectsDrawerElement() : base()
    {
        AddToClassList("rects-drawer-element");

        this.focusable = true;
        this.RegisterCallback<KeyDownEvent>(OnKeyDownEvent);

        this.RegisterCallback<FocusEvent>(OnFocus);
        this.RegisterCallback<BlurEvent>(OnBlur);
    }

    public void BindProperty(SerializedProperty rectsProperty)
    {
        this.rectsProperty = rectsProperty;
    }

    private void OnBlur(BlurEvent evt)
    {
        DisplayFocussed = false;
    }

    private void OnFocus(FocusEvent evt)
    {
        DisplayFocussed = true;
    }

    private void OnKeyDownEvent(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Delete && this.selectedRectIndex != -1)
        {
            List<Rect> list = Rects.ToList();
            list.RemoveAt(this.selectedRectIndex);
            Rects = list.ToArray();
            this.manipulationRect = default;
            this.selectedRectIndex = -1;
            base.MarkDirtyRepaint();
        }
    }

    public override void OnGenerateVisualContent(MeshGenerationContext context)
    {
        base.OnGenerateVisualContent(context);

        DrawRects(context);
    }

    void DrawRects(MeshGenerationContext context)
    {
        Rect[] rects = Rects;

        if (rects == null)
            return;

        MeshContainer rectsContainer = new MeshContainer(context);

        for (int i = 0; i < rects.Length; i++)
            if (this.selectedRectIndex == i)
                continue;
            else
                rectsContainer.AddRect(ToElementSpace(rects[i]), this.rectColor);


        switch (this.toolState)
        {
            case ToolState.Drawing:
                rectsContainer.AddRect(ToElementSpace(this.manipulationRect), this.manipulationRectColor);
                break;

            default:
                if(this.manipulationRect != default)
                    rectsContainer.AddRect(ToElementSpace(this.manipulationRect), this.selectedColor);
                break;
        }
    }        

    public override void OnMouseDown(MouseDownEvent evt)
    {
        base.OnMouseDown(evt);
        
        if(evt.pressedButtons == 1)
        {
            /// Select/Deselect
            this.selectedRectIndex = SelectionIndex(Rects, base.MouseGridPosition);        
        
            switch(this.selectedRectIndex)
            {
                case -1:
                    this.toolState = ToolState.Drawing;                    
                    this.manipulationRect = new Rect(base.MouseSnappedGridPosition, Vector2.zero);
                    break;
                default:
                    this.manipulationRect = Rects[this.selectedRectIndex];
                    this.dragOffset = base.MouseSnappedGridPosition - this.manipulationRect.position;
                    this.toolState = ToolState.Dragging;
                    break;
            }

            base.MarkDirtyRepaint();
        }        
    }

    public override void OnMouseMove(MouseMoveEvent evt)
    {
        base.OnMouseMove(evt);

        if(evt.pressedButtons == 1)
        {
            switch (this.toolState)
            {
                case ToolState.Resizing:
                case ToolState.Drawing:
                    this.manipulationRect.size = base.MouseSnappedGridPosition - this.manipulationRect.position;                    
                    break;

                case ToolState.Dragging:
                    this.manipulationRect.position = base.MouseSnappedGridPosition - this.dragOffset;
                    break;
            }

            base.MarkDirtyRepaint();
        }
    }

    public override void OnMouseUp(MouseUpEvent evt)
    {
        base.OnMouseUp(evt);
        Rect[] array = Rects;

        if(evt.button == 0)
        {
            switch (this.toolState)
            {
                case ToolState.Drawing:
                    if (!RectValid(this.manipulationRect))
                        break;
                    
                    this.manipulationRect = CleanupRect(this.manipulationRect);

                    List<Rect> list = Rects.ToList();
                    list.Add(new Rect(this.manipulationRect));
                    array = list.ToArray();

                    this.selectedRectIndex = array.Length - 1;                    
                    break;

                case ToolState.Dragging:
                    array[this.selectedRectIndex].position = this.manipulationRect.position;                    
                    break;

                case ToolState.Resizing:
                    this.manipulationRect = CleanupRect(this.manipulationRect);
                    if (RectValid(this.manipulationRect))                    
                        array[this.selectedRectIndex] = this.manipulationRect;                                          
                    else
                        this.manipulationRect = Rects[this.selectedRectIndex];
                    break;
            }

        }

        Rects = array;
        base.MarkDirtyRepaint();
        this.toolState = ToolState.None;
    }

    bool RectValid(Rect rect)
    {
        if (rect == null)
            return false;

        if (rect.width == 0 || rect.height == 0)
            return false;

        return true;
    }

    Rect CleanupRect(Rect rect)
    {
        if (rect.width < 0)
        {
            rect.x += rect.width;
            rect.width *= -1;
        }

        if (rect.height < 0)
        {
            rect.y += rect.height;
            rect.height *= -1;
        }

        return rect;
    }

    int SelectionIndex(Rect[] rects, Vector2 mousePosition)
    {
        if(rects == null || rects.Length == 0)
            return -1;

        for (int i = 0; i < rects.Length; i++)
            if (rects[i].Contains(mousePosition))
                return i;

        return -1;
    }
}
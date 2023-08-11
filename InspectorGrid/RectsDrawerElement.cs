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


    Rect[] localRectsArray = new Rect[1] {new Rect(0, 0, 20, 100)};

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

    ToolState toolState = ToolState.None;
    Rect manipulationRect = default;
    SerializedProperty rectsProperty = null;
    Color selectedColor = Color.red;
    Color rectColor = Color.cyan;
    Color manipulationRectColor = Color.yellow;
    int selectedRectIndex = -1;
    Vector2 dragOffset = Vector2.zero;


    public new class UxmlFactory : UxmlFactory<RectsDrawerElement, UxmlTraits> { }



    public RectsDrawerElement() : base()
    {
        AddToClassList("rects-drawer-element");

        this.RegisterCallback<KeyDownEvent>(OnKeyDownEvent);
    }

    private void OnKeyDownEvent(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Delete && this.selectedRectIndex != -1)
        {
            List<Rect> list = Rects.ToList();
            list.RemoveAt(this.selectedRectIndex);
            Rects = list.ToArray();
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
                rectsContainer.AddRect(ToElementSpace(this.manipulationRect), this.selectedColor, 0.0f, 5.0f);
            else
                rectsContainer.AddRect(ToElementSpace(rects[i]), this.rectColor, 0.0f, 5.0f);

        if (this.toolState == ToolState.Drawing)            
            rectsContainer.AddRect(ToElementSpace(this.manipulationRect), this.manipulationRectColor, 0.0f, 5.0f);

        Debug.Log(ToElementSpace(this.manipulationRect));
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
                    Rects = list.ToArray();

                    this.selectedRectIndex = list.Count - 1;                    
                    break;

                case ToolState.Dragging:                    
                    Rects[this.selectedRectIndex].position = this.manipulationRect.position;         
                    break;

                case ToolState.Resizing:
                    this.manipulationRect = CleanupRect(this.manipulationRect);
                    if (RectValid(this.manipulationRect))
                        Rects[this.selectedRectIndex] = this.manipulationRect;
                    else
                        this.manipulationRect = Rects[this.selectedRectIndex];
                    break;
            }

        }

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

//public class RectElement : VisualElement
//{
//    Rect rect = default;

//    public Rect Rect
//    {
//        get => rect;
//        set
//        {
//            rect = value;
            
//        }
//    }

//    Matrix4x4 matrix = default;

//    public new class UxmlFactory : UxmlFactory<RectElement, UxmlTraits> { }

//    public RectElement()
//    {
//        AddToClassList("rect-element");
//    }

//    void SetPosition()
//}

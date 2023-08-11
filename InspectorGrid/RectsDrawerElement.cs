using System.Collections;
using System.Collections.Generic;
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

    Rect[] Rects
    {
        get
        {
            List<Rect> list = new List<Rect>();
            for (int i = 0; i < this.rectsProperty.arraySize; i++)
            {
                list.Add(this.rectsProperty.GetArrayElementAtIndex(i).rectValue);
            }
            return list.ToArray();
        }

        set
        {
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
    }

    public override void OnGenerateVisualContent(MeshGenerationContext context)
    {
        base.OnGenerateVisualContent(context);

        DrawRects(context);
    }

    void DrawRects(MeshGenerationContext context)
    {
        if (this.rectsProperty == null)
            return;

        Rect[] rects = Rects;

        if (rects == null || rects.Length == 0)
            return;

        MeshContainer rectsContainer = new MeshContainer(context);

        for (int i = 0; i < rects.Length; i++)
            if(this.selectedRectIndex == i)
                rectsContainer.AddRect(this.manipulationRect, this.selectedColor, -1.0f, 2.0f);
            else
                rectsContainer.AddRect(rects[i], this.rectColor, -1.0f, 2.0f);
                
        if (this.toolState == ToolState.Drawing)
            rectsContainer.AddRect(this.manipulationRect, this.manipulationRectColor, -1.0f, 2.0f);
    }

    public override void OnMouseDown(MouseDownEvent evt)
    {
        base.OnMouseDown(evt);

        if(evt.pressedButtons != 1)
            return;

        /// Select/Deselect
        int selection = SelectionIndex(Rects, base.MouseGridPosition);
        if(selection != this.selectedRectIndex)
        {
            switch(selection)
            {
                case -1:
                    this.toolState = ToolState.Drawing;
                    this.manipulationRect = new Rect(base.MouseSnappedGridPosition, Vector2.zero);
                    break;
                default:
                    this.manipulationRect = Rects[selection];
                    this.dragOffset = base.MouseGridPosition - this.manipulationRect.position;
                    this.toolState = ToolState.Dragging;
                    break;
            }

            this.selectedRectIndex = selection;
            base.MarkDirtyRepaint();
            return;
        }
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

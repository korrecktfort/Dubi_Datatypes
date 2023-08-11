using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Dubi.GridElements;
using System.Linq;


[CustomPropertyDrawer(typeof(RectsContainer), true)]
public class RectsContainerDrawer : GridEditorDrawer
{
    enum DragState
    {
        Dragging,
        Drawing,
        Resizing,
        None,
    }

    protected SerializedProperty rects = null;

    protected Rect[] Rects
    {
        get
        {
            List<Rect> list = new List<Rect>();
            for (int i = 0; i < this.rects.arraySize; i++)            
                list.Add(this.rects.GetArrayElementAtIndex(i).rectValue);
            
            return list.ToArray();
        }

        set
        {
            this.rects.arraySize = value.Length;
            for (int i = 0; i < value.Length; i++)            
                this.rects.GetArrayElementAtIndex(i).rectValue = value[i];
            
            this.rects.serializedObject.ApplyModifiedProperties();
        }
    }

    Vector2 ContextMousePos { get => this.snapToGrid ? ClampedGridMousePos : GridMousePos; }

    int selectedIndex = -1;
    DragState dragState = DragState.None;
    Rect maniRect = new Rect();
    Vector2 dragOffset = Vector2.zero;
    bool snapToGrid = true;
    float resizeSelectionRange = 0.25f;
    GUIStyle fontStyle = new GUIStyle();
    float localFontScale = 0.70f;
    bool mouseInside = false;    

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        this.rects = property.FindPropertyRelative("rects");
        this.snapToGrid = property.FindPropertyRelative("snapToGrid").boolValue;        

        this.fontStyle.alignment = TextAnchor.UpperLeft;

        return base.CreatePropertyGUI(property);
    }

    public override void DrawHandles()
    {
        this.fontStyle.fontSize = (int)((float)base.FontSize * this.localFontScale);

        base.DrawHandles();

        DrawRectsContainerHandles();
    }

    public virtual void DrawRectsContainerHandles()
    {
        Rect[] array = Rects;
        Color yellow = Color.yellow;
        yellow.a = 0.25f;

        for (int i = 0; i < array.Length; i++)
        {
            yellow.a = 1.0f;
            Color outer = i == selectedIndex ? yellow : Color.cyan;
            yellow.a = 0.25f;
            Color inner = i == selectedIndex ? yellow : Color.clear;

            if (this.selectedIndex == i)
                DrawRect(this.maniRect, inner, outer, i);
            else
                DrawRect(array[i], Color.clear, outer, i);
        }

        if (this.dragState == DragState.Drawing)
            DrawRect(this.maniRect, yellow, Color.yellow, -1);

        if (this.selectedIndex > -1)
        {
            bool inRange = this.mouseInside && this.dragState == DragState.Resizing || InResizeManiRange();
            using (new Handles.DrawingScope(inRange ? Color.cyan : Color.yellow))
                Handles.DrawSolidDisc(this.maniRect.max, Vector3.forward, this.resizeSelectionRange);
        }
    }

    public void DrawRect(Rect rect, Color inner, Color outer, int index)
    {
        Handles.DrawSolidRectangleWithOutline(rect, inner, outer);
        DisplayRectInfo(rect, outer, index);        

    }

    void DisplayRectInfo(Rect rect, Color c, int index)
    {
        this.fontStyle.normal.textColor = c;
        Handles.Label(rect.min + Vector2.right * 0.05f,
            "p" + rect.position.ToString("F0") + "\n"
            + "s" + rect.size.ToString("F0") + "\n"
            + "i:" + index, this.fontStyle);        
    }

    public override void OnMouseDown(MouseDownEvent e)
    {
        base.OnMouseDown(e);

        switch(e.button)
        {
            case 0:
                Rect[] array = Rects;
                Vector2 mouseGridPos = GridMousePos;

                if (InResizeManiRange())
                {
                    this.dragState = DragState.Resizing;
                    return;
                }
                    

                SelectRect(array, mouseGridPos);

                switch (this.selectedIndex)
                {
                    case -1:
                        this.dragState = DragState.Drawing;
                        this.maniRect = new Rect(ContextMousePos, Vector2.zero);
                        break;

                    default:
                        this.maniRect = Rects[this.selectedIndex];
                        this.dragOffset = ContextMousePos - this.maniRect.position;
                        this.dragState = DragState.Dragging;
                        break;
                }
                break;
        }
    }

    public override void OnMouseMove(MouseMoveEvent e)
    {
        base.OnMouseMove(e);

        switch (e.button)
        {
            case 0:
                switch (this.dragState)
                {
                    case DragState.Resizing:                        
                    case DragState.Drawing:
                        Vector2 v = ContextMousePos - this.maniRect.position;
                        if (this.snapToGrid)
                            this.maniRect.size = new Vector2(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
                        else
                            this.maniRect.size = v;
                        break;

                    case DragState.Dragging:
                        this.maniRect.position = ContextMousePos - this.dragOffset;
                        break;

                    default:                        
                        break;
                }
                break;
        }
    }

    public override void OnMouseUp(MouseUpEvent e)
    {
        base.OnMouseUp(e);

        switch (e.button)
        {
            case 0:
                Rect[] array = this.Rects;

                switch (this.dragState)
                {
                    case DragState.Drawing:
                        this.maniRect = CleanupRect(this.maniRect);
                        if (RectValid(this.maniRect))
                        {
                            List<Rect> list = array.ToList();
                            list.Add(new Rect(this.maniRect));
                            array = list.ToArray();
                            OnRectAdded();
                            this.selectedIndex = list.Count - 1;
                            OnSelectedIndex(this.selectedIndex);
                        }
                        this.Rects = array;
                        this.dragState = DragState.None;
                        break;

                    case DragState.Dragging:                             
                        array[this.selectedIndex].position = this.maniRect.position;
                        this.Rects = array;
                        this.dragState = DragState.None;
                        break;

                    case DragState.Resizing:
                        this.maniRect = CleanupRect(this.maniRect);
                        if (RectValid(this.maniRect))
                            array[this.selectedIndex] = new Rect(this.maniRect);
                        else
                            this.maniRect = array[this.selectedIndex];

                        this.Rects = array;
                        this.dragState = DragState.None;
                        break;

                    default:
                        this.Rects = array;
                        this.dragState = DragState.None;
                        break;
                }

               
                break;
        }
    }

    public override void OnKeyDown(KeyDownEvent e)
    {
        base.OnKeyDown(e);

        switch (this.dragState)
        {
            case DragState.None:
                if (e.keyCode == KeyCode.Delete && this.selectedIndex != -1)
                {
                    List<Rect> list = Rects.ToList();
                    list.RemoveAt(this.selectedIndex);
                    OnRectRemovedAt(this.selectedIndex);
                    this.Rects = list.ToArray();
                    this.selectedIndex = -1;
                    OnSelectedIndex(this.selectedIndex);
                }
                break;
        }
    }

    Rect CleanupRect(Rect rect)
    {
        if(rect.size.x < 0.0f)
        {
            float sizeX = -rect.size.x;
            float x = rect.x - sizeX;

            rect.x = x;
            rect.size = new Vector2(sizeX, rect.size.y);
        }

        if(rect.size.y < 0.0f)
        {
            float sizeY = -rect.size.y;
            float y = rect.y - sizeY;

            rect.y = y;
            rect.size = new Vector2(rect.size.x, sizeY);
        }

        return rect;
    }



    bool RectValid(Rect rect)
    {
        if (rect.size.x > 0.0f && rect.size.y > 0.0f)
            return true;

        return false;
    }

    bool InsideRect(Rect[] array, Vector2 pos)
    {
        foreach (Rect rect in array)
            if (rect.Contains(pos))
                return true;

        return false;
    }

    void SelectRect(Rect[] array, Vector2 pos)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].Contains(pos))
            {

                /// Toggle same selection
                //if(this.selectedIndex == i)
                //{
                //    this.selectedIndex = -1;
                //    return;
                //}

                this.selectedIndex = i;
                OnSelectedIndex(this.selectedIndex);
                return;
            }
        }

        this.selectedIndex = -1;
        OnSelectedIndex(this.selectedIndex);
    }

    public virtual void OnSelectedIndex(int index)
    {
    }

    bool InResizeManiRange()
    {
        if (this.selectedIndex > -1)
        {
            if (Vector2.Distance(this.maniRect.max, GridMousePos) < this.resizeSelectionRange)
            {                
                return true;
            }
        }

        return false;
    }

    public virtual void OnRectAdded()
    {
    }

    public virtual void OnRectRemovedAt(int index)
    {
    }

    public override void OnMouseEnter(MouseEnterEvent e)
    {
        base.OnMouseEnter(e);
        this.mouseInside = true;
    }

    public override void OnMouseLeave(MouseLeaveEvent e)
    {
        base.OnMouseLeave(e);
        this.mouseInside = false;
    }
}
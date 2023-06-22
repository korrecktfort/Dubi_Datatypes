using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using Dubi.GridElements;

[CustomPropertyDrawer(typeof(GridEditorProperty))]
public abstract class GridEditorDrawer : PropertyDrawer
{
    PersistentGridElement gridElement = null;
    VisualElement toolsArea = null;
    Vector2 gridMousePos = Vector2.zero;    
    Vector2 clampedGridMousePos = Vector2.zero;
    int fontSize = 1;

    protected Vector2 GridMousePos { get => this.gridMousePos; }
    protected Vector2 ClampedGridMousePos { get => this.clampedGridMousePos; }
    protected StyleCursor StyleCursor { set => this.gridElement.StyleCursor = value; }
    protected int FontSize { get => this.fontSize; }
    protected VisualElement ToolsArea { get => this.toolsArea; }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new VisualElement();
        root.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        root.style.flexWrap = new StyleEnum<Wrap>(Wrap.Wrap);

        this.gridElement = new PersistentGridElement(            
            property,
            OnMouseDown,
            OnMouseUp,
            OnMouseEnter,
            OnMouseLeave,
            OnMouseWheel,
            OnKeyDown,
            DrawHandles,
            OnMousePosOnGridChanged,
            OnMouseMove,
            OnFontSizeChanged
            );

        this.toolsArea = this.gridElement.Q<VisualElement>("ToolsArea");

        root.Add(this.gridElement);
        return root;
    }   

    public virtual void OnMouseMove(MouseMoveEvent e)
    {
    }

    public virtual void OnMouseDown(MouseDownEvent e)
    {
    }

    public virtual void OnMouseUp(MouseUpEvent e)
    {
    }

    public virtual void OnMouseEnter(MouseEnterEvent e)
    {
    }

    public virtual void OnMouseLeave(MouseLeaveEvent e)
    {
    }

    public virtual void OnMouseWheel(WheelEvent e)
    {
    }

    public virtual void OnKeyDown(KeyDownEvent e)
    {
    }

    void OnMousePosOnGridChanged(Vector2 mousePos, Vector2 clampedMousePos)
    {
        this.gridMousePos = mousePos;
        this.clampedGridMousePos = clampedMousePos;
    }

    public virtual void DrawHandles()
    {
       
    }   

    void OnFontSizeChanged(int fontSize)
    {
        this.fontSize = fontSize;
    }
}
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using Dubi.GridElements;

public class PersistentGridElement : GridElement
{
    GridEditorProperty.GridStart gridStart = GridEditorProperty.GridStart.TopLeft;
    bool draggable = true;
    bool snapToGrid = true;
    bool drawMousePosOnGrid = false;

    Vector2 originFontPos = Vector2.one;
    GUIStyle originFontStyle = new GUIStyle();
    float fontScale = 0.41f;
    protected Vector2 gridMousePos = Vector2.zero;

    Action<Vector2> OnGridOffsetChanged;
    Action<float> OnZoomChanged;

    protected Action<MouseMoveEvent> OnMouseMove;
    protected Action<MouseDownEvent> OnMouseDown;
    protected Action<MouseUpEvent> OnMouseUp;
    protected Action<MouseEnterEvent> OnMouseEnter;
    protected Action<MouseLeaveEvent> OnMouseLeave;
    protected Action<WheelEvent> OnMouseWheel;
    protected Action<KeyDownEvent> OnKeyDown;
    protected Action OnDrawHandles;
    protected Action<Vector2, Vector2> OnMousePosOnGridChanged;
    protected Action<int> OnFontSizeChanged;

    public StyleCursor StyleCursor
    {
        set => base.style.cursor = value;
    }

    public PersistentGridElement(SerializedProperty property) : this(property, null, null, null, null, null, null, null, null, null, null)
    {
    }

    public PersistentGridElement(SerializedProperty property, Action<MouseDownEvent> OnMouseDown, Action<MouseUpEvent> OnMouseUp, Action<MouseEnterEvent> OnMouseEnter, Action<MouseLeaveEvent> OnMouseLeave, Action<WheelEvent> OnMouseWheel, Action<KeyDownEvent> OnKeyDown, Action DrawHandles, Action<Vector2, Vector2> OnMousePosOnGridChanged, Action<MouseMoveEvent> OnMouseMove, Action<int> OnFontSizeChanged) : base()
    {
        VisualElement toolsArea = new VisualElement() { name = "ToolsArea" };
        toolsArea.style.position = new StyleEnum<Position>(Position.Absolute);
        toolsArea.style.left = new StyleLength(0.0f);
        toolsArea.style.bottom = new StyleLength(0.0f);
        base.Add(toolsArea);

        #region Apply Properties
        SerializedProperty gridStartProp = property.FindPropertyRelative("gridStart");
        SerializedProperty draggableProp = property.FindPropertyRelative("draggableGrid");
        SerializedProperty gridOffsetProp = property.FindPropertyRelative("gridOffset");
        SerializedProperty zoomProp = property.FindPropertyRelative("zoom");
        SerializedProperty snapToGrid = property.FindPropertyRelative("snapToGrid");

        Vector2 elementSize = property.FindPropertyRelative("elementSize").vector2Value;
        base.focusable = true;

        this.gridStart = (GridEditorProperty.GridStart)gridStartProp.enumValueIndex;
        this.draggable = draggableProp.boolValue;
        this.gridOffset = gridOffsetProp.vector2Value;
        this.zoom = zoomProp.floatValue;
        this.snapToGrid = snapToGrid.boolValue;

        if (this.gridOffset == Vector2.zero)
        {
            float horOffset = elementSize.x * 0.5f - this.zoom;
            float verOffset = elementSize.y * 0.5f - this.zoom;

            switch (gridStart)
            {
                case GridEditorProperty.GridStart.Center:
                    this.gridOffset = Vector2.zero;
                    break;

                case GridEditorProperty.GridStart.TopLeft:
                    this.gridOffset = new Vector2(-horOffset, -verOffset);
                    break;

                case GridEditorProperty.GridStart.TopRight:
                    this.gridOffset = new Vector2(horOffset, -verOffset);
                    break;

                case GridEditorProperty.GridStart.BottomRight:
                    this.gridOffset = new Vector2(horOffset, verOffset);
                    break;

                case GridEditorProperty.GridStart.BottomLeft:
                    this.gridOffset = new Vector2(-horOffset, verOffset);
                    break;
            }
        }

        gridOffsetProp.vector2Value = this.gridOffset;
        gridOffsetProp.serializedObject.ApplyModifiedProperties();
        #endregion

        #region Style
        float margin = 3.0f;
        float borderWidth = 2.0f;
        Color borderColor = Color.grey;
        float borderRadius = 2.0f;

        this.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
        this.style.position = new StyleEnum<Position>(Position.Relative);
        this.style.width = new StyleLength(elementSize.x);
        this.style.height = new StyleLength(elementSize.y);
        this.style.marginTop = new StyleLength(margin);
        this.style.marginRight = new StyleLength(margin);
        this.style.marginBottom = new StyleLength(margin);
        this.style.marginLeft = new StyleLength(margin);
        this.style.borderTopWidth = new StyleFloat(borderWidth);
        this.style.borderRightWidth = new StyleFloat(borderWidth);
        this.style.borderBottomWidth = new StyleFloat(borderWidth);
        this.style.borderLeftWidth = new StyleFloat(borderWidth);
        this.style.borderTopColor = new StyleColor(borderColor);
        this.style.borderRightColor = new StyleColor(borderColor);
        this.style.borderBottomColor = new StyleColor(borderColor);
        this.style.borderLeftColor = new StyleColor(borderColor);
        this.style.borderTopLeftRadius = new StyleLength(borderRadius);
        this.style.borderTopRightRadius = new StyleLength(borderRadius);
        this.style.borderBottomRightRadius = new StyleLength(borderRadius);
        this.style.borderBottomLeftRadius = new StyleLength(borderRadius);

        this.originFontPos = new Vector2(-2.4f, 0.1f);
        switch (this.gridStart)
        {
            case GridEditorProperty.GridStart.TopLeft:
                this.originFontPos = new Vector2(-2.4f, -1.6f);
                break;
            case GridEditorProperty.GridStart.BottomRight:
                this.originFontPos = new Vector2(0.0f, 0.1f);
                break;
            case GridEditorProperty.GridStart.TopRight:
                this.originFontPos = new Vector2(0.0f, -1.6f);
                break;

            case GridEditorProperty.GridStart.Center:
            case GridEditorProperty.GridStart.BottomLeft:
                this.originFontPos = new Vector2(-2.4f, 0.1f);
                break;
        }
        #endregion

        #region Register Callbacks
        RegisterCallback<MouseMoveEvent>((e) =>
        {
            if (this.draggable && e.pressedButtons == 4)
            {
                this.OnGridOffsetChanged?.Invoke(this.gridOffset);

                gridOffsetProp.vector2Value = this.gridOffset;
                gridOffsetProp.serializedObject.ApplyModifiedProperties();
            }

            UpdateMousePosition(e.localMousePosition);

            this.OnMouseMove?.Invoke(e);
            base.MarkDirtyRepaint();
        });

        RegisterCallback<MouseDownEvent>((e) =>
        {
            this.drawMousePosOnGrid = true;
            UpdateMousePosition(e.localMousePosition);
            this.OnMouseDown?.Invoke(e);
            base.MarkDirtyRepaint();
        });

        RegisterCallback<MouseUpEvent>((e) =>
        {
            this.drawMousePosOnGrid = false;
            UpdateMousePosition(e.localMousePosition);
            this.OnMouseUp?.Invoke(e);
            base.MarkDirtyRepaint();
        });

        RegisterCallback<MouseEnterEvent>((e) =>
        {
            Focus();
            UpdateMousePosition(e.localMousePosition);
            this.OnMouseEnter?.Invoke(e);
            base.MarkDirtyRepaint();
        });

        RegisterCallback<MouseLeaveEvent>((e) =>
        {
            this.drawMousePosOnGrid = false;
            UpdateMousePosition(e.localMousePosition);
            this.OnMouseLeave?.Invoke(e);
            base.MarkDirtyRepaint();
        });

        RegisterCallback<WheelEvent>((e) =>
        {
            UpdateMousePosition(e.localMousePosition);

            if (e.delta.y != 0.0f)
            {
                this.zoom = Mathf.Clamp(this.zoom - e.delta.y, 2.0f, 101.0f);
                FocusOnPosition(this.gridMousePos);

                zoomProp.floatValue = this.zoom;
                zoomProp.serializedObject.ApplyModifiedProperties();
            }

            this.OnMouseWheel?.Invoke(e);
            this.OnZoomChanged?.Invoke(this.zoom);
            e.StopImmediatePropagation();
            base.MarkDirtyRepaint();
        });

        RegisterCallback<KeyDownEvent>((e) =>
        {
            this.OnKeyDown?.Invoke(e);
            base.MarkDirtyRepaint();
        });
        #endregion

        #region Register Delegates
        this.OnMouseMove = OnMouseMove;
        this.OnMouseDown = OnMouseDown;
        this.OnMouseUp = OnMouseUp;
        this.OnMouseEnter = OnMouseEnter;
        this.OnMouseLeave = OnMouseLeave;
        this.OnMouseWheel = OnMouseWheel;
        this.OnKeyDown = OnKeyDown;
        this.OnDrawHandles = DrawHandles;
        this.OnMousePosOnGridChanged = OnMousePosOnGridChanged;
        this.OnFontSizeChanged = OnFontSizeChanged;
        #endregion

        base.MarkDirtyRepaint();
    }

    void UpdateMousePosition(Vector2 localMousePosition)
    {
        Vector2 gridMousePos = (localMousePosition - (Vector2)GridMatrix().MultiplyPoint(Vector2.zero)) / this.zoom;
        Vector2 clampedMousePos = new Vector2(Mathf.RoundToInt(gridMousePos.x), Mathf.RoundToInt(gridMousePos.y));

        this.gridMousePos = this.snapToGrid ? clampedMousePos : gridMousePos;
        this.OnMousePosOnGridChanged?.Invoke(gridMousePos, clampedMousePos);
    }

    protected override void DrawHandles()
    {
        Matrix4x4 m = GridMatrix();

        /// Draw Origin Handles
        using (new Handles.DrawingScope(Color.grey, m))
        {
            int fontSize = (int)(this.zoom * this.fontScale);
            this.originFontStyle.fontSize = fontSize;
            this.OnFontSizeChanged?.Invoke(fontSize);
            this.originFontStyle.normal.textColor = Handles.color;
            Handles.Label(this.originFontPos * this.fontScale, "(0|0)", this.originFontStyle);
            Handles.DrawSolidDisc(Vector3.zero, Vector3.forward, 0.1f);
        }

        if (this.drawMousePosOnGrid)
            using (new Handles.DrawingScope(new Color(0.85f, 0.85f, 0.85f, 1.0f), m))
                Handles.DrawSolidDisc(this.gridMousePos, Vector3.forward, 0.1f);

        using (new Handles.DrawingScope(Color.white, m))
            this.OnDrawHandles?.Invoke();
    }
}
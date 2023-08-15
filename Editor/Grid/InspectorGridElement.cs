using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorGridElement : VisualElement
{
    float zoom = 1;
    float zoomStep = 0.1f;
    Vector2 offset = Vector2.zero;
    int paddingCorrect = 0;

    int firstGridPpu = 10;
    Color firstGridColor;
    int firstGridThickness = 1;

    int secondGridPpu = 100;
    Color secondGridColor;
    int secondGridThickness = 1;

    Color centerGridColor;

    Vector2 localMousePos = Vector2.zero;
    Vector2 mouseGridPosition = Vector2.zero;
    Vector2 mouseSnappedGridPosition = Vector2.zero;
    Vector2 gridPosition = Vector2.zero;
    int posSize = 1;

    int mouseSnapPixels = 10;
    bool offsetSetup = false;

    Label mouseCoordinateLabel = null;

    public Vector2 LocalMousePosition
    {
        get => this.localMousePos;

        set
        {
            this.localMousePos = value;
            this.mouseSnappedGridPosition = this.mouseGridPosition = ScaleToGridCoordinate(GridMatrix.inverse.MultiplyPoint(value));

            this.mouseSnappedGridPosition.x = Mathf.Round(this.mouseGridPosition.x / this.mouseSnapPixels) * this.mouseSnapPixels;
            this.mouseSnappedGridPosition.y = Mathf.Round(this.mouseGridPosition.y / this.mouseSnapPixels) * this.mouseSnapPixels;

            this.gridPosition = this.mouseGridPosition / this.firstGridPpu;

            if(this.mouseCoordinateLabel != null)
                this.mouseCoordinateLabel.text = $"Mouse: {this.mouseSnappedGridPosition.ToString("F2")}";
        }
    }

    public Vector2 MouseGridPosition => this.mouseGridPosition;

    public Vector2 MouseSnappedGridPosition => this.mouseSnappedGridPosition;

    /// In Element Space
    Vector2 GridCenter => new Vector2(this.layout.width * 0.5f, this.layout.height * 0.5f) + this.offset;    

    Matrix4x4 GridMatrix => Matrix4x4.TRS(this.GridCenter, Quaternion.identity, Vector3.one * this.zoom);


    #region Grid Element Settings Properties
    public float Zoom
    {
        get => zoom;
        set
        {
            zoom = value;
            base.MarkDirtyRepaint();
        }
    }

    public float ZoomStep
    {
        get => zoomStep;
        set => zoomStep = value;        
    }

    public Vector2 Offset
    {
        get => offset;
        set
        {
            offset = value;
            base.MarkDirtyRepaint();
        }
    }

    public int MouseSnapPixels
    {
        get => mouseSnapPixels;
        set => mouseSnapPixels = value;
    }
#endregion

    #region Grid Settings Properties
    public int PaddingCorrect
    {
        get => paddingCorrect;
        set
        {
            paddingCorrect = value;
            base.MarkDirtyRepaint();
        }
    }

    public int FirstGridPpu 
    { 
        get => firstGridPpu;
        set
        {
            firstGridPpu = value;
            base.MarkDirtyRepaint();
        }
    }

    public int SecondGridPpu
    {
        get => secondGridPpu;
        set
        {
            secondGridPpu = value;
            base.MarkDirtyRepaint();
        }
    }      

    public Color FirstGridColor
    {
        get => firstGridColor;
        set
        {
            firstGridColor = value;
            base.MarkDirtyRepaint();
        }
    }

    public Color SecondGridColor
    {
        get => secondGridColor;
        set
        {
            secondGridColor = value;
            base.MarkDirtyRepaint();
        }
    }

    public int FirstGridThickness
    {
        get => firstGridThickness;
        set
        {
            firstGridThickness = value;
            base.MarkDirtyRepaint();
        }
    }

    public int SecondGridThickness
    {
        get => secondGridThickness;
        set
        {
            secondGridThickness = value;
            base.MarkDirtyRepaint();
        }
    }

    public Color CenterGridColor { get => centerGridColor; set => centerGridColor = value; }
    #endregion

    #region Uxml Classes
    public new class UxmlFactory : UxmlFactory<InspectorGridElement, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {               
        UxmlFloatAttributeDescription zoom = new UxmlFloatAttributeDescription { name = "zoom", defaultValue = 1.0f };
        UxmlFloatAttributeDescription zoomStep = new UxmlFloatAttributeDescription { name = "zoom-step", defaultValue = 0.1f };
        /// <summary>
        /// generally shift the grid offset for correcting paddings created by e.g. border style settings
        /// </summary>
        UxmlIntAttributeDescription paddingCorrect = new UxmlIntAttributeDescription {name = "padding-correct", defaultValue = 0 };
        UxmlIntAttributeDescription mouseSnapPixels = new UxmlIntAttributeDescription { name = "mouse-snap-pixels", defaultValue = 10 };

        UxmlColorAttributeDescription centerGridColor = new UxmlColorAttributeDescription { name = "center-grid-color", defaultValue = Color.white };

        /// <summary>
        /// First Grid Settings
        /// </summary>
        UxmlIntAttributeDescription firstGridPpu = new UxmlIntAttributeDescription { name = "first-grid-ppu", defaultValue = 10 };
        UxmlColorAttributeDescription firstGridColor = new UxmlColorAttributeDescription { name = "first-grid-color", defaultValue = Color.white };
        UxmlIntAttributeDescription firstGridThickness = new UxmlIntAttributeDescription { name = "first-grid-thickness", defaultValue = 1 };

        /// <summary>
        /// Second Grid Settings
        /// </summary>
        UxmlIntAttributeDescription secondGridPpu = new UxmlIntAttributeDescription { name = "second-grid-ppu", defaultValue = 100 };
        UxmlColorAttributeDescription secondGridColor = new UxmlColorAttributeDescription { name = "second-grid-color", defaultValue = Color.white };
        UxmlIntAttributeDescription secondGridThickness = new UxmlIntAttributeDescription { name = "second-grid-thickness", defaultValue = 2 };


        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            InspectorGridElement element = (InspectorGridElement)ve;        
            element.Zoom = zoom.GetValueFromBag(bag, cc);
            element.ZoomStep = zoomStep.GetValueFromBag(bag, cc);
            element.PaddingCorrect = paddingCorrect.GetValueFromBag(bag, cc);
            element.MouseSnapPixels = mouseSnapPixels.GetValueFromBag(bag, cc);
            element.CenterGridColor = centerGridColor.GetValueFromBag(bag, cc);

            element.FirstGridPpu = firstGridPpu.GetValueFromBag(bag, cc);
            element.FirstGridColor = firstGridColor.GetValueFromBag(bag, cc);
            element.FirstGridThickness = firstGridThickness.GetValueFromBag(bag, cc);

            element.SecondGridPpu = secondGridPpu.GetValueFromBag(bag, cc);
            element.SecondGridColor = secondGridColor.GetValueFromBag(bag, cc);
            element.SecondGridThickness = secondGridThickness.GetValueFromBag(bag, cc);
        }
    }
    #endregion

    public InspectorGridElement()
    {
        AddToClassList("inspector-grid-element");
        this.generateVisualContent = OnGenerateVisualContent;

        this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        this.RegisterCallback<WheelEvent>(OnWheel);    
        
        this.RegisterCallback<MouseDownEvent>(OnMouseDown); 
        this.RegisterCallback<MouseUpEvent>(OnMouseUp);
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    public void InjectMouseCoordinateLabel(Label label)
    {
        this.mouseCoordinateLabel = label;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        SetGridOffset(true, Vector2.one * 20.0f);
    }

    void SetGridOffset(bool force, Vector2 localPosition)
    {
        if (!this.offsetSetup || force)
        {
            this.offset = -new Vector2(this.layout.width * 0.5f, this.layout.height * 0.5f) + localPosition;
            this.offsetSetup = true;
        }
    }

    public void OffsetFocusGridPosition(Vector2 gridPosition)
    {
        /// Set offset to focus element position
        this.offset = /*-new Vector2(this.layout.width * 0.5f, this.layout.height * 0.5f)*/ - ScaleToGridPixel(gridPosition) * this.zoom;
        base.MarkDirtyRepaint();
    }

    public virtual void OnWheel(WheelEvent evt)
    {
        if(evt.mouseDelta.y == 0.0f)
            return;

        /// OffsetChange - Cache Grid Position for later use
        Vector2 cachedGridPosition = this.mouseSnappedGridPosition;
        Vector2 elementPosBefore = GridToElementSpace(cachedGridPosition);

        Vector2 cached = this.gridPosition;

        this.zoom += evt.mouseDelta.y > 0 ? -this.zoomStep : this.zoomStep;
        this.zoom = Mathf.Clamp(this.zoom, this.zoomStep, 5.0f);

        /// Zoom Changed, Update Grid Position
        this.LocalMousePosition = evt.localMousePosition;

        /// Offset Change - Determine offset change to focus mouse position after zoom
        Vector2 elementPosAfter = GridToElementSpace(cachedGridPosition);
        Vector2 delta = elementPosAfter - elementPosBefore;
        this.offset -= delta;

        evt.StopImmediatePropagation();
        base.MarkDirtyRepaint();
    }

    public virtual void OnMouseMove(MouseMoveEvent evt)
    {
        this.LocalMousePosition = evt.localMousePosition;

        if (evt.pressedButtons == 4)
        {
            this.offset += evt.mouseDelta;

            evt.StopImmediatePropagation();
        }

        base.MarkDirtyRepaint();
    }

    public virtual void OnMouseDown(MouseDownEvent evt)
    {
        this.LocalMousePosition = evt.localMousePosition;
        this.CaptureMouse();
    }

    public virtual void OnMouseUp(MouseUpEvent evt)
    {
        this.ReleaseMouse();
        this.LocalMousePosition = evt.localMousePosition;
    }

    public virtual void OnGenerateVisualContent(MeshGenerationContext context)
    {
        DrawGrid(context, this.firstGridPpu, this.firstGridColor, this.firstGridThickness);
        DrawGrid(context, this.secondGridPpu, this.SecondGridColor, this.secondGridThickness);
        
        MeshContainer centerGrid = new MeshContainer(context);
        float size = 2.0f;
        Rect rect = new Rect(GridCenter - Vector2.one * size * 0.5f, Vector2.one * size);
        centerGrid.AddRect(rect, this.centerGridColor, 0.0f, size);        
        
        // DrawMousePosition(context);               
    }

    void DrawMousePosition(MeshGenerationContext context)
    {
        context.DrawText(this.gridPosition.ToString("F0"), this.localMousePos - Vector2.up * this.firstGridPpu, this.firstGridPpu, Color.red);

        MeshContainer mouse = new MeshContainer(context);
        Vector2 pos = GridMatrix.MultiplyPoint3x4(new Vector2(this.mouseGridPosition.x, this.mouseGridPosition.y));        
        Vector2 size = Vector2.one * this.posSize * this.zoom;
        Rect rect = new Rect(pos - size * 0.5f, size);
        mouse.AddRect(rect, Color.red);
    }    

    void DrawGrid(MeshGenerationContext context, float factor, Color color, float thickness)
    {
        float size = this.zoom * factor;

        if (size <= 5) /// grid is super small and appears super messy, abort
            return;

        int xCount = Mathf.CeilToInt(base.contentRect.width / size) + 1;
        int yCount = Mathf.CeilToInt(base.contentRect.height / size) + 1;

        Vector2 preOffset = this.offset + base.contentRect.size * 0.5f + Vector2.one * this.paddingCorrect;
        Vector2 offset = new Vector2(preOffset.x % size, preOffset.y % size);

        List<Line2D> lines = new List<Line2D>();
        for (int x = 0; x < xCount; x++)
        {
            Vector3 start = offset + new Vector2(size * x, -size);

            if(start.x < base.layout.xMin || start.x > base.layout.xMax)
                continue;

            Vector3 end = offset + new Vector2(size * x, base.contentRect.height + size);

            start.y = base.layout.yMin;
            end.y = base.layout.yMax;

            lines.Add(new Line2D(start, end, ToGridPixelPosition(start).x == 0.0f ? this.centerGridColor : color));
        }

        for (int y = 0; y < yCount; y++)
        {
            Vector3 start = offset + new Vector2(-size, size * y);

            if(start.y < base.layout.yMin || start.y > base.layout.yMax)
                continue;

            Vector3 end = offset + new Vector2(base.contentRect.width + size, size * y);

            start.x = base.layout.xMin;
            end.x = base.layout.xMax;

            lines.Add(new Line2D(start, end, ToGridPixelPosition(start).y == 0.0f ? this.centerGridColor : color));
        }

        MeshContainer grid = new MeshContainer(context);
        foreach(Line2D line in lines)
            grid.AddLine(line, line.Color, 0.0f, thickness);
    }

    #region Transformations
    Rect ToElementSpace(Rect rect)
    {
        return new Rect(ToElementPosition(rect.position), rect.size * this.zoom);
    }

    Rect ToGridSpace(Rect rect)
    {
        return new Rect(ToGridPixelPosition(rect.position), rect.size / this.zoom);
    }

    public Rect ElementToGridSpace(Rect rect)
    {
        return ToGridSpace(new Rect(ScaleToGridCoordinate(rect.position), ScaleToGridCoordinate(rect.size)));
    }

    public Rect GridToElementSpace(Rect rect)
    {
        return ToElementSpace(new Rect(ScaleToGridPixel(rect.position), ScaleToGridPixel(rect.size)));
    }

    public Vector2 GridToElementSpace(Vector2 v)
    {
        return ToElementPosition(ScaleToGridPixel(v));
    }

    public Vector2 ElementToGridSpace(Vector2 v)
    {
        return ToGridPixelPosition(ScaleToGridCoordinate(v));
    }

    //public Vector2 FromGridCoordinateToElementPosition(Vector2 gridCoordinate) => this.GridCenter + FromGridCoordinate(gridCoordinate) * this.zoom;

    //public Vector2 FromElementPositionToGridCoordinate(Vector2 elementPosition) => ToGridCoordinate(elementPosition - this.GridCenter) * this.zoom;

    Vector2 ToElementPosition(Vector2 gridPosition) => this.GridCenter + gridPosition * this.zoom;

    Vector2 ToGridPixelPosition(Vector2 localElementPosition) => (localElementPosition - this.GridCenter) / this.zoom;

    Vector2 ScaleToGridCoordinate(Vector2 value) => value / this.firstGridPpu;

    Vector2 ScaleToGridPixel(Vector2 value) => value * this.firstGridPpu;

    //Vector2 FromGridCoordinate(Vector2 gridCoordinate) => gridCoordinate * this.firstGridPpu;

    Vector2[] ToElementPosition(Vector2[] gridPositions)
    {
        Vector2 gridCenter = this.GridCenter;
        for (int i = 0; i < gridPositions.Length; i++)
        {
            gridPositions[i] = gridCenter + gridPositions[i];
        }

        return gridPositions;
    }
    #endregion

  
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Bson;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class RectsDrawerElement : InspectorGridElement
{
    #region Enum
    enum ToolState
    {
        Dragging,
        Drawing,
        Resizing,
        None,
    }
    #endregion

    #region Properties
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
    #endregion

    #region Fields
    Rect[] localRectsArray = new Rect[0];

    SerializedProperty rectsProperty = null;
    ToolState toolState = ToolState.None;
    Rect manipulationRect = default;
    Color selectedColor = Color.red;
    Color rectColor = Color.cyan;
    Color manipulationRectColor = Color.yellow;
    int selectedRectIndex = -1;
    Vector2 dragOffset = Vector2.zero;
    float mouseResizeRange = 15.0f;
    ListView listView = null;
    #endregion

    #region UXML Factory
    public Color SelectedColor { get => selectedColor; set => selectedColor = value; }
    public Color RectColor { get => rectColor; set => rectColor = value; }
    public Color ManipulationRectColor { get => manipulationRectColor; set => manipulationRectColor = value; }
    public float MouseResizeRange { get => mouseResizeRange; set => mouseResizeRange = value; }

    public new class UxmlFactory : UxmlFactory<RectsDrawerElement, UxmlTraits> { }

    public new class UxmlTraits : InspectorGridElement.UxmlTraits
    {
        UxmlColorAttributeDescription selectedRectColor = new UxmlColorAttributeDescription { name = "selected-color", defaultValue = Color.red };

        UxmlColorAttributeDescription rectColor = new UxmlColorAttributeDescription { name = "rect-color", defaultValue = Color.cyan };

        UxmlColorAttributeDescription manipulationRectColor = new UxmlColorAttributeDescription { name = "manipulation-rect-color", defaultValue = Color.yellow };

        UxmlFloatAttributeDescription mouseResizeRange = new UxmlFloatAttributeDescription { name = "mouse-resize-range", defaultValue = 10.0f };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            RectsDrawerElement element = (RectsDrawerElement)ve;
            element.SelectedColor = selectedRectColor.GetValueFromBag(bag, cc);
            element.RectColor = rectColor.GetValueFromBag(bag, cc);
            element.ManipulationRectColor = manipulationRectColor.GetValueFromBag(bag, cc);
            element.MouseResizeRange = mouseResizeRange.GetValueFromBag(bag, cc);
        }
    }
    #endregion

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

    #region List View & Events

    public void InjectListView(ListView listView)
    {
        listView.selectedIndicesChanged += OnSelectedIndicesChanged;
        listView.itemsSourceChanged += OnItemsSourceChanged;     
        listView.RegisterCallback<SerializedPropertyChangeEvent>(OnSerializedPropertyChanged);
        listView.itemIndexChanged += OnItemIndexChanged;
        listView.itemsRemoved += ItemsRemoved;
        this.listView = listView;
    }

    private void ItemsRemoved(IEnumerable<int> obj)
    {
        this.selectedRectIndex = -1;
        this.manipulationRect = default;

        base.MarkDirtyRepaint();
    }

    private void OnItemIndexChanged(int arg1, int arg2)
    {
        if (this.selectedRectIndex > -1)
            this.manipulationRect = Rects[this.selectedRectIndex];

        base.MarkDirtyRepaint();
    }

    public void InjectLabel(Label label)
    {
        label.text = this.rectsProperty.displayName + ":";
    }

    private void OnSerializedPropertyChanged(SerializedPropertyChangeEvent evt)
    {
        if (this.selectedRectIndex >= -1)
            this.manipulationRect = Rects[this.selectedRectIndex];

        base.MarkDirtyRepaint();
    }

    private void OnItemsSourceChanged()
    {
        base.MarkDirtyRepaint();
    }

    private void OnSelectedIndicesChanged(IEnumerable<int> enumerable)
    {
        if (enumerable.Count() == 0)
            return;        

        SelectRect(enumerable.FirstOrDefault());        
    }
    #endregion

    #region Visuals
    public override void OnGenerateVisualContent(MeshGenerationContext context)
    {
        base.OnGenerateVisualContent(context);

        DrawRects(context);
        DrawManiResizeRect(context);

        /// Mani Rect Position
        if (this.toolState == ToolState.Drawing)
            DrawMiniRect(context, this.manipulationRect.position, 0.1f, this.manipulationRectColor);

        /// Mouse Position
        DrawMiniRect(context, base.MouseSnappedGridPosition, 0.1f, Color.red);
    }

    void DrawRects(MeshGenerationContext context)
    {
        Rect[] rects = Rects;

        if (rects == null)
            return;

        MeshContainer rectsContainer = new MeshContainer(context);

        for (int i = 0; i < rects.Length; i++)
            if (this.selectedRectIndex == i)
                DrawRectInfo(rectsContainer.Context, rects[i], this.selectedColor, true);
            else
                DrawRect(rectsContainer, rects[i], this.rectColor);


        switch (this.toolState)
        {
            case ToolState.Drawing:                    
                    DrawRect(rectsContainer, this.manipulationRect, this.manipulationRectColor);
                break;
                

            default:
                if(this.manipulationRect != default)
                    DrawRect(rectsContainer, this.manipulationRect, this.selectedColor);                
                break;
        }
    }
       
    void DrawRect(MeshContainer container, Rect rect, Color color, bool skipRectInfo = false)
    {
        container.AddRect(GridToElementSpace(rect), color);
                
        if(!skipRectInfo)
            DrawRectInfo(container.Context, rect, color);
    }
        
    void DrawRectInfo(MeshGenerationContext context, Rect rect, Color color, bool skipRectInfo = false)
    {
        if (skipRectInfo)
            return; 

        int index = IndexOf(rect);
        string indexString = "i:" + (index == -1 ? "n/a" : index.ToString());
        string rectData = "p:(" + rect.position.x.ToString("F0") + "|" + rect.position.y.ToString("F0") + ")\ns:(" + rect.width.ToString("F0") + "|" + rect.height.ToString("F0") + ")\n" + indexString;

        context.DrawText(rectData, GridToElementSpace(rect.position), 12.0f, color);
    }    

    void DrawManiResizeRect(MeshGenerationContext context)
    {
        if (this.manipulationRect == default)
            return;

        if(this.selectedRectIndex == -1)
            return;

        Color color = this.toolState == ToolState.Resizing || InResizeManiRange() ? this.selectedColor : this.manipulationRectColor;

        DrawMiniRect(context, this.manipulationRect.max, 0.3f, color);       
    }

    void DrawMiniRect(MeshGenerationContext context, Vector2 position, float size, Color color)
    {
        MeshContainer meshContainer = new MeshContainer(context);        
        Rect rect = new Rect(position - new Vector2(size, size) * 0.5f, new Vector2(size, size));
        meshContainer.AddRect(GridToElementSpace(rect), color, 0.0f, 3.0f);
    }
    #endregion

    #region Own Events
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
            this.toolState = ToolState.None;

            List<Rect> list = Rects.ToList();
            list.RemoveAt(this.selectedRectIndex);
            Rects = list.ToArray();
            this.manipulationRect = default;
            this.selectedRectIndex = -1;
            base.MarkDirtyRepaint();
        }
    }

    public override void OnMouseDown(MouseDownEvent evt)
    {
        base.OnMouseDown(evt);
        
        if(evt.pressedButtons == 1)
        {
            if (InResizeManiRange())
            {
                this.toolState = ToolState.Resizing;
                return;
            }

            /// Select/Deselect
            SelectRect(SelectionIndex(Rects, base.MouseGridPosition));
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

                    if (this.selectedRectIndex == -1)
                        break;

                    Rect[] array = Rects;
                    array[selectedRectIndex] = CleanupRect(this.manipulationRect);
                    Rects = array;

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
                    {
                        this.manipulationRect = default;
                        break;
                    }
                    
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
    #endregion

    #region Functions
    void SelectRect(int index)
    {
        this.selectedRectIndex = index;

        switch (this.selectedRectIndex)
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

        if (this.listView == null)
            return;

        this.listView.selectedIndex = this.selectedRectIndex;
    }

    bool InResizeManiRange()
    {
        if(this.selectedRectIndex == -1)
            return false;

        if(this.manipulationRect == default)
            return false;
                
        return Vector2.Distance(this.manipulationRect.max, base.MouseSnappedGridPosition) < this.mouseResizeRange;
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

    int IndexOf(Rect rect)
    {
        List<Rect> list = Rects.ToList();

        if (list.Contains(rect))
            return Rects.ToList().IndexOf(rect);

        return -1;
    }
    #endregion
}
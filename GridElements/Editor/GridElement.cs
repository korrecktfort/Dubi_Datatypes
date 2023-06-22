using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections.Generic;

namespace Dubi.GridElements
{
    public class GridElement : ImmediateModeElement
    {
        protected VisualElement elementsContainer = new VisualElement();

        protected float pixelsPerUnit = 10.0f;
        protected float pixelsPerUnitFactor = 0.1f;
        protected float zoom = 25.0f;
        protected Vector2 gridOffset = Vector2.one;
        protected Vector2 originFactor = Vector2.one;
        protected Color gridColor = new Color(255, 255, 255, 0.1f);

        public GridElement()
        {
            this.elementsContainer.name = "Visual Element Container";
            this.elementsContainer.style.position = new StyleEnum<Position>(Position.Absolute);
            Add(this.elementsContainer);

            //this.gridContainer.style.top = new StyleLength(0.0f);
            //this.gridContainer.style.right = new StyleLength(0.0f);
            //this.gridContainer.style.bottom = new StyleLength(0.0f);
            //this.gridContainer.style.left = new StyleLength(0.0f);
            //this.gridContainer.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);

            this.style.position = new StyleEnum<Position>(Position.Absolute);
            this.style.top = new StyleLength(0.0f);
            this.style.right = new StyleLength(0.0f);
            this.style.bottom = new StyleLength(0.0f);
            this.style.left = new StyleLength(0.0f);

            RegisterCallback<MouseMoveEvent>((e) =>
            {
                if (e.pressedButtons == 4)
                {
                    this.gridOffset += e.mouseDelta;
                }

                base.MarkDirtyRepaint();
            });

            this.pixelsPerUnitFactor = 1.0f / this.pixelsPerUnit;

            base.MarkDirtyRepaint();
        }

        protected override void ImmediateRepaint()
        {
            /// Re-Position onGridElementOrigin
            Vector2 origin = GridMatrix().MultiplyPoint3x4(Vector2.zero);
            this.elementsContainer.style.left = new StyleLength(origin.x);
            this.elementsContainer.style.top = new StyleLength(origin.y);
            this.elementsContainer.style.scale = new StyleScale(new Scale(Vector3.one * this.zoom * 0.1f));

            Color colorGridSmall = this.gridColor;
            Color colorGridBig = colorGridSmall;
            colorGridBig.a *= 2.0f;

            Handles.BeginGUI();

            DrawGrid(1.0f, colorGridSmall); /// Small Grid
            DrawGrid(this.pixelsPerUnit, colorGridBig); /// Big Grid

            DrawHandles();

            Handles.EndGUI();
        }

        protected void DrawGrid(float gridFactor, Color color)
        {
            float gridSize = this.zoom * gridFactor;
            Vector2 preOffset = this.gridOffset + base.contentRect.size * 0.5f;

            int xCount = Mathf.CeilToInt(base.contentRect.width / gridSize) + 1;
            int yCount = Mathf.CeilToInt(base.contentRect.height / gridSize) + 1;
            Vector2 offset = new Vector2(preOffset.x % gridSize, preOffset.y % gridSize);
            Handles.color = color;

            for (int i = 0; i < xCount; i++)
            {
                Vector3 start = offset + new Vector2(gridSize * i, -gridSize);
                Vector3 end = offset + new Vector2(gridSize * i, base.contentRect.height + gridSize);
                Handles.DrawLine(start, end);
            }

            for (int h = 0; h < yCount; h++)
            {
                Vector3 start = offset + new Vector2(-gridSize, gridSize * h);
                Vector3 end = offset + new Vector2(base.contentRect.width + gridSize, gridSize * h);
                Handles.DrawLine(start, end);
            }
        }

        protected Matrix4x4 GridMatrix()
        {
            Vector2 offset = this.gridOffset + base.contentRect.size * 0.5f;
            return Matrix4x4.TRS(offset, Quaternion.identity, this.originFactor * this.zoom);
        }

        public void FocusOnPosition(Vector2 gridPosition, float zoom = -1)
        {
            this.gridOffset = -gridPosition;

            if (zoom > -1)
                this.zoom = zoom;
        }

        protected virtual void DrawHandles() { }

        protected void AddToElementsContainer(VisualElement visualElement)
        {
            if (visualElement != null && !this.elementsContainer.Contains(visualElement))
                this.elementsContainer.Add(visualElement);
        }

        public void RemoveFromElementsContainer(VisualElement visualElement)
        {
            if (visualElement != null && this.elementsContainer.Contains(visualElement))
                this.elementsContainer.Remove(visualElement);
        }

        protected Vector2 WorldToGrid(Vector2 worldPos)
        {
            return this.elementsContainer.WorldToLocal(worldPos) * this.pixelsPerUnitFactor;
        }

        public Vector2 ElementCenterToGrid(VisualElement visualElement)
        {
            return this.elementsContainer.WorldToLocal(visualElement.LocalToWorld(visualElement.contentRect.center)) * this.pixelsPerUnitFactor;
        }
    }
}
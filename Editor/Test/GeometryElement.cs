using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GeometryElement : VisualElement
{
    float lineThickness = 10f;
    float lineOffset = 0.0f;
    Color lineColor;

    public float LineThickness
    {
        get => this.lineThickness;
        set
        {
            this.lineThickness = value;
            this.MarkDirtyRepaint();
        }
    }

    public float LineOffset 
    { 
        get => this.lineOffset;
        set 
        { 
            this.lineOffset = value; 
            this.MarkDirtyRepaint();
        }
    }

    public Color LineColor 
    { 
        get => lineColor;
        set 
        { 
            lineColor = value; 
            this.MarkDirtyRepaint();
        }
    }

    public new class UxmlFactory : UxmlFactory<GeometryElement, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlIntAttributeDescription lineThickness = new UxmlIntAttributeDescription { name = "line-thickness", defaultValue = 1 };

        UxmlIntAttributeDescription lineOffset = new UxmlIntAttributeDescription { name = "line-offset", defaultValue = 0 };

        UxmlColorAttributeDescription lineColor = new UxmlColorAttributeDescription { name = "line-color", defaultValue = Color.white };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            GeometryElement geometryElement = (GeometryElement)ve;
            geometryElement.LineThickness = lineThickness.GetValueFromBag(bag, cc);
            geometryElement.LineOffset = lineOffset.GetValueFromBag(bag, cc);
            geometryElement.LineColor = lineColor.GetValueFromBag(bag, cc);
        }
    }

    public GeometryElement()
    {
        this.AddToClassList("geometry-element");
        base.generateVisualContent = GenerateVisualContent;

        this.RegisterCallback<MouseDownEvent>(MouseDown);
    }

    List<Vector2> points = new List<Vector2>();
    private void MouseDown(MouseDownEvent evt)
    {
        if (evt.button != 0)
            return;

        this.points.Add(evt.localMousePosition);

        this.MarkDirtyRepaint();
    }

    

    private void GenerateVisualContent(MeshGenerationContext context)
    {
        MeshContainer containerOne = new MeshContainer(context);
        containerOne.AddRect(new Rect(0, 0, 99, 99), this.lineColor, this.lineOffset, this.lineThickness);
        containerOne.AddRect(new Rect(100, 100, 99, 99), this.lineColor, this.lineOffset, this.lineThickness);

        //MeshContainer containerTwo = new MeshContainer(context);
        //containerTwo.AddRect(new Rect(100, 100, 80, 80), this.lineColor, this.lineOffset, this.lineThickness);

        //meshGeneration.AddLines(this.points.ToArray(), this.lineColor, this.lineOffset, this.lineThickness, true);
        //meshGeneration.AddLines(this.points.ToArray(), Color.red, 0.0f, 1.0f);

        // Debug.Log(meshGeneration.VertexCount);
    }

}

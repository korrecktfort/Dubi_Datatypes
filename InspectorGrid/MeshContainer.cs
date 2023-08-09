using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshContainer
{
    MeshGenerationContext context;
    Vertex[] vertices = new Vertex[0];
    ushort[] indices = new ushort[0];
    float pixelOffset = 1.0f;

    public int VertexCount => this.vertices == null ? 0 : this.vertices.Length;

    public MeshContainer(MeshGenerationContext context)
    {
        this.context = context;
    }

    public void AddRect(Rect rect, Color color, float offset = 0.0f, float thickness = 1.0f)
    {
        if (rect == null)
            return;

        Vector2[] points = new Vector2[4];
        points[0] = rect.position;
        points[1] = rect.position + new Vector2(rect.width, 0.0f);
        points[2] = rect.position + rect.size;
        points[3] = rect.position + new Vector2(0.0f, rect.height);

        AddLines(points, color, offset, thickness, true); 
    }

    /// Adding Lines to the Mesh, Not Connecting to the last Line -> using more vertices
    #region Stack Processing Not Connected
    public void AddLines(Vector2[][] points, Color color, float thickness = 1.0f)
    {
        if (points == null || points.Length == 0)
            return;

        for (int i = 0; i < points.Length; i++)
            AddLines(points[i], color, 0.0f, thickness);
    }

    public void AddLines(Vector2[] points, Color color, float thickness = 1.0f, bool circle = false)
    {
        if (points == null || points.Length == 0)
            return;

        for (int i = 0; i < points.Length - 1; i++)
            AddLine(points[i], points[i + 1], color, 0.0f, thickness);

        if (circle)
            AddLine(points[points.Length - 1], points[0], color, 0.0f, thickness);
    }

    public void AddLines(Vector2[] points, Color color, float offset, float thickness = 1.0f, bool circle = false)
    {
        if (points == null || points.Length == 0)
            return;

        Line2D[] lines = Line2D.CreateLines2D(points, circle);
        AddLines(lines, color, offset, thickness);
    }

    public void AddLines(Line2D[] lines, Color color, float offset, float thickness = 1.0f)
    {
        if (lines == null || lines.Length == 0)
            return;

        for (int i = 0; i < lines.Length; i++)
            AddLine(lines[i], color, offset, thickness);
    }
    #endregion

    /// Adding Lines to the Mesh, Connecting to the last Line -> using less vertices
    #region Stack Processing Connected

    public void AddLinesConnect(Vector2[][] points, Color color, float thickness = 1.0f)
    {
        if(points == null || points.Length == 0)
            return;

        for (int i = 0; i < points.Length; i++)
            AddLinesConnect(points[i], color, 0.0f, thickness);
    }

    public void AddLinesConnect(Vector2[] points, Color color, float thickness = 1.0f, bool circle = false)
    {
        if(points == null || points.Length == 0)
            return;

        for (int i = 0; i < points.Length - 1; i++)
            AddLine(points[i], points[i + 1], color, 0.0f, thickness, true);

        if (circle)
            AddLine(points[points.Length - 1], points[0], color, 0.0f, thickness, true);
    }

    public void AddLinesConnect(Vector2[] points, Color color, float offset, float thickness = 1.0f, bool circle = false)
    {
        if(points == null || points.Length == 0)
            return;

        Line2D[] lines = Line2D.CreateLines2D(points, circle);
        AddLinesConnect(lines, color, offset, thickness);
    }

    public void AddLinesConnect(Line2D[] lines, Color color, float offset, float thickness = 1.0f)
    {
        if(lines == null || lines.Length == 0)
            return; 

        for (int i = 0; i < lines.Length; i++)
            AddLine(lines[i], color, offset, thickness, true);
    }
    #endregion

    void AddLine(Line2D line, Color color, float offset, float thickness = 1.0f, bool connected = false)
    {
        thickness = Mathf.Max(thickness, 1.0f);

        Vector3 p1 = (Vector3)line.End;
        Vector3 end = new Vector3(p1.x, p1.y, Vertex.nearZ);
        Vector3 normalEnd = (Vector3)line.NormalEnd * thickness * Mathf.Max(0.5f, Mathf.Abs(Vector2.Dot(line.Dir.normalized, line.NormalEnd)));
        Vector3 offsetEnd = (Vector3)line.NormalEnd * offset;

        if (thickness % 2 != 0)
            end += new Vector3(this.pixelOffset, this.pixelOffset, Vertex.nearZ);

        List<Vertex> vertices = new List<Vertex>(this.vertices);

        if (this.vertices.Length == 0 || !connected)
        {
            Vector3 p0 = (Vector3)line.Start;
            Vector3 start = new Vector3(p0.x, p0.y, Vertex.nearZ);
            Vector3 normalStart = (Vector3)line.NormalStart * thickness * Mathf.Max(0.5f, Mathf.Abs(Vector2.Dot(line.Dir.normalized, line.NormalStart)));
            Vector3 offsetStart = (Vector3)line.NormalStart * offset;

            if (thickness % 2 != 0)
                start += new Vector3(this.pixelOffset, this.pixelOffset, Vertex.nearZ);

            vertices.Add(new Vertex() { position = start - normalStart + offsetStart, tint = color });
            vertices.Add(new Vertex() { position = start + normalStart + offsetStart, tint = color });
        }

        vertices.Add(new Vertex() { position = end - normalEnd + offsetEnd, tint = color });
        vertices.Add(new Vertex() { position = end + normalEnd + offsetEnd, tint = color });

        this.vertices = vertices.ToArray();

        List<ushort> indices = new List<ushort>(this.indices);
        int index = this.vertices.Length - 1;

        indices.Add((ushort)(index - 3));
        indices.Add((ushort)(index - 1));
        indices.Add((ushort)(index - 2));

        indices.Add((ushort)(index - 2));
        indices.Add((ushort)(index - 1));
        indices.Add((ushort)(index - 0));
        this.indices = indices.ToArray();

        /// meshwritedata has to be in the context of the VisualElement.GenerateVisualContent() method
        MeshWriteData meshWriteData = this.context.Allocate(this.vertices.Length, this.indices.Length);
        
        meshWriteData.SetAllVertices(this.vertices);
        meshWriteData.SetAllIndices(this.indices);
    }

    void AddLine(Vector2 p0, Vector2 p1, Color color, float offset = 0.0f, float thickness = 1.0f, bool connected = false)
    {
        thickness = Mathf.Max(thickness, 1.0f);

        Vector3 v0 = new Vector3(p0.x, p0.y, Vertex.nearZ);
        Vector3 v1 = new Vector3(p1.x, p1.y, Vertex.nearZ);

        Vector3 dir = v1 - v0;        
        Vector3 normal = new Vector3(-dir.y, dir.x).normalized * thickness * 0.5f;       
        Vector3 offsetV = normal * offset;

        if(thickness % 2 != 0)
        {
            v0 += new Vector3(this.pixelOffset, this.pixelOffset, Vertex.nearZ);
            v1 += new Vector3(this.pixelOffset, this.pixelOffset, Vertex.nearZ);
        }
               
        List<Vertex> vertices = new List<Vertex>(this.vertices);

        if(this.vertices.Length == 0 || !connected)
        {
            vertices.Add(new Vertex() { position = v0 - normal + offsetV, tint = color });
            vertices.Add(new Vertex() { position = v0 + normal + offsetV, tint = color });
        }

        vertices.Add(new Vertex() { position = v1 - normal + offsetV, tint = color });
        vertices.Add(new Vertex() { position = v1 + normal + offsetV, tint = color });
        
        this.vertices = vertices.ToArray();
        
        List<ushort> indices = new List<ushort>(this.indices);
        int index = this.vertices.Length - 1;

        indices.Add((ushort)(index - 3));
        indices.Add((ushort)(index - 1));
        indices.Add((ushort)(index - 2));
        indices.Add((ushort)(index - 2));
        indices.Add((ushort)(index - 1));
        indices.Add((ushort)(index - 0));
        this.indices = indices.ToArray();

        /// meshwritedata has to be in the context of the VisualElement.GenerateVisualContent() method
        MeshWriteData meshWriteData = this.context.Allocate(this.vertices.Length, this.indices.Length);
        meshWriteData.SetAllVertices(this.vertices);
        meshWriteData.SetAllIndices(this.indices);
    }   
}



using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshContainer
{
    MeshGenerationContext context;
    Vertex[] vertices = new Vertex[0];
    ushort[] indices = new ushort[0];
    //float pixelOffset = 1.0f;

    public int VertexCount => this.vertices == null ? 0 : this.vertices.Length;

    public MeshContainer(MeshGenerationContext context)
    {
        this.context = context;
    }

    public void AddRects(Rect[] rects, Color color, float offset = 0.0f, float thickness = 1.0f)
    {
        if (rects == null || rects.Length == 0)
            return;

        for (int i = 0; i < rects.Length; i++)
            AddRect(rects[i], color, offset, thickness);
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

        AddLines(points, color, thickness, true); 
    }

    /// Adding Lines to the Mesh, Not Connecting to the last Line -> using more vertices
    #region Stack Processing Not Connected
    //public void AddLines(Vector2[][] points, Color color, float thickness = 1.0f)
    //{
    //    if (points == null || points.Length == 0)
    //        return;

    //    for (int i = 0; i < points.Length; i++)
    //        AddLines(points[i], color, 0.0f, thickness);
    //}

    //public void AddLines(Vector2[] points, Color color, float thickness = 1.0f, bool circle = false)
    //{
    //    if (points == null || points.Length == 0)
    //        return;

    //    for (int i = 0; i < points.Length - 1; i++)
    //        AddLine(points[i], points[i + 1], color, 0.0f, thickness);

    //    if (circle)
    //        AddLine(points[points.Length - 1], points[0], color, 0.0f, thickness);
    //} 

    public void AddLines(Vector2[] points, Color color, float thickness = 1.0f, bool circle = false)
    {
        if (points == null || points.Length == 0)
            return;

        for (int i = 0; i < points.Length - 1; i++)
            AddLine(points[i], points[i + 1], color, 0.0f, thickness);

        if (circle)
            AddLine(points[points.Length - 1], points[0], color, 0.0f, thickness);
    }

    public void AddLines(Vector2[] points, Color[] color, float thickness = 1.0f, bool circle = false)
    {
        if (points == null || points.Length == 0)
            return;

        for (int i = 0; i < points.Length - 1; i++)
            AddLine(points[i], points[i + 1], color[i], 0.0f, thickness);

        if (circle)
            AddLine(points[points.Length - 1], points[0], color[color.Length - 1], 0.0f, thickness);
    }

    public void AddLines(Vector2[] points, Color color, float offset, float thickness = 1.0f, bool circle = false)
    {
        if (points == null || points.Length == 0)
            return;

        Line2D[] lines = Line2D.CreateLines2D(points, circle);
        AddLines(lines, color, offset, thickness);
    }

    public void AddLines(Line2D[] lines, Color color, float offset = 0.0f, float thickness = 1.0f)
    {
        if (lines == null || lines.Length == 0)
            return;

        for (int i = 0; i < lines.Length; i++)
            AddLine(lines[i], color, offset, thickness);
    }

    public void AddLinesNotConnected(Vector2[] points, Color[] colors, float offset = 0.0f, float thickness = 1.0f, bool circle = false)
    {
        if(points == null || points.Length == 0)
            return;

        Line2D[] lines = Line2D.CreateLines2D(points, colors, circle);
        
        foreach(Line2D line in lines)
            AddLine(line, line.Color, offset, thickness, false);
    }
    #endregion

    /// Adding Lines to the Mesh, Connecting to the last Line -> using less vertices
    #region Stack Processing Connected
    public void AddLinesConnect(Vector2[] points, Color color, float offset = 0.0f, float thickness = 1.0f, bool circle = false)
    {
        if(points == null || points.Length == 0)
            return;

        Line2D[] lines = Line2D.CreateLines2D(points, circle);
        AddLinesConnect(lines, color, offset, thickness);        
    }

    public void AddLinesConnect(Vector2[] points, Color[] colors, float offset = 0.0f, float thickness = 1.0f, bool circle = false)
    {
        if (points == null || points.Length == 0)
            return;

        Line2D[] lines = Line2D.CreateLines2D(points, colors, circle);

        foreach(Line2D line in lines)
            AddLine(line, line.Color, offset, thickness, true);
    }

    public void AddLinesConnect(Line2D[] lines, Color color, float offset = 0.0f, float thickness = 1.0f)
    {
        if(lines == null || lines.Length == 0)
            return; 

        for (int i = 0; i < lines.Length; i++)
            AddLine(lines[i], color, offset, thickness, true);
    }
    #endregion

    public void AddLine(Line2D line, Color color, float offset, float thickness = 1.0f, bool connected = false)
    {      
        AddLine(line.Start, line.End, color, line.NormalStart, line.NormalEnd, offset, thickness, connected);
    }

    public void AddLine(Vector2 p0, Vector2 p1, Color color, float offset = 0.0f, float thickness = 1.0f, bool connected = false)
    {
        AddLine(p0, p1, color, Vector2.zero, Vector2.zero, offset, thickness, connected);
    }

    void AddLine(Vector2 p0, Vector2 p1, Color color, Vector2 normalStart, Vector2 normalEnd, float offset = 0.0f, float thickness = 1.0f, bool connected = false)
    {
        Vector3 l0 = new Vector3(p0.x, p0.y, Vertex.nearZ);
        Vector3 l1 = new Vector3(p1.x, p1.y, Vertex.nearZ);

        Vector3 dir = (l1 - l0).normalized;

        /// grow line to consider thickness
        l0 -= dir * thickness * 0.5f;
        l1 += dir * thickness * 0.5f;

        Vector3 normal = Vector3.Cross(Vector3.forward, dir);
        
        if (normalStart == Vector2.zero)
            normalStart = normal;
        
        if (normalEnd == Vector2.zero)
            normalEnd = normal;

        float startDot = Mathf.Abs(Vector2.Dot(dir, normalStart));
        float endDot = Mathf.Abs(Vector2.Dot(dir, normalEnd));
                
        float thicknessStart = Mathf.Lerp(thickness, thickness * 2.0f, startDot);
        float thicknessEnd = Mathf.Lerp(thickness, thickness * 2.0f, endDot);

        // width        
        Vector3 widthStart = normalStart * thicknessStart;
        Vector3 widthEnd = normalEnd * thicknessEnd;

        // additional offset
        Vector3 offsetStart = normalStart * offset;
        Vector3 offsetEnd = normalEnd * offset;     
               
        List<Vertex> vertices = new List<Vertex>(this.vertices);

        if(this.vertices.Length == 0 || !connected)
        {
            vertices.Add(new Vertex() { position = l0 - widthStart * 0.5f + offsetStart, tint = color });
            vertices.Add(new Vertex() { position = l0 + widthStart * 0.5f + offsetStart, tint = color });
        }

        vertices.Add(new Vertex() { position = l1 - widthEnd * 0.5f + offsetEnd, tint = color });
        vertices.Add(new Vertex() { position = l1 + widthEnd * 0.5f + offsetEnd, tint = color });
        
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

    bool IsFlipped(Vector3 normal)
    {
        if(normal.x < 0.0f || normal.y < 0.0f)
        {
            return true;
        }

        return false;
    }

    Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
}



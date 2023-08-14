﻿using UnityEngine;

public class Line2D
{
    /// <summary>
    /// p0 - The start point of the line
    /// p1 - The end point of the line
    /// normal - The normal of the line at its center
    /// normalp0 - The normal of the line at p0
    /// normalp1 - The normal of the line at p1
    /// dir - The direction of the line
    /// </summary>
    Vector2 p0, p1, dir, normalp0, normal, normalp1;
    Line2D previous, next;
    Color color = Color.white;

    public Vector2 Start => this.p0;

    public Vector2 End => this.p1;

    public Vector2 Normal => this.normal;

    public Vector2 Dir => this.dir;

    public Vector2 NormalStart => this.normalp0;

    public Vector2 NormalEnd => this.normalp1;

    public bool HasPrevious => this.previous != null;

    public bool HasNext => this.next != null;

    public Line2D Previous
    {
        set
        {
            Vector2 normal = Vector2.Lerp(value.normal, this.normal, 0.5f);        

            /// The normal of the previous line at p1 is the same as the normal of the current line at p0
            value.normalp1 = normal;

            /// The normal of the current line at p0 is the same as the normal of the previous line at p1
            this.normalp0 = normal;            

            /// set the relations to each other
            value.next = this;
            this.previous = value;
        }
    }

    public Color Color => color; 

    public Line2D (Vector2 p0, Vector2 p1, Color color) : this(p0, p1)
    {
        this.color = color;
    }

    public Line2D(Vector2 p0, Vector2 p1)
    {
        this.p0 = p0;
        this.p1 = p1;

        Vector2 v = p1 - p0;
        this.dir = v.normalized;
        this.normal = new Vector2(-v.y, v.x).normalized;
        this.normalp0 = this.normal;
        this.normalp1 = this.normal;
    }

    public static Line2D[] CreateLines2D(Vector2[] points, bool circle = false)
    {
        if (points.Length <= 0)
            return null;

        Line2D[] lines = circle ? new Line2D[points.Length] : new Line2D[points.Length - 1];

        for (int i = 0; i < points.Length - 1; i++)
            lines[i] = new Line2D(points[i], points[i + 1]);

        for (int i = 1; i < points.Length - 1; i++)
            lines[i].Previous = lines[i - 1];

        if (circle)
        {
            lines[lines.Length - 1] = new Line2D(points[points.Length - 1], points[0]);
            lines[lines.Length - 1].Previous = lines[lines.Length - 2];
            lines[0].Previous = lines[lines.Length - 1];
        }

        return lines;
    }

    public static Line2D[] CreateLines2D(Vector2[] points, Color[] colors, bool circle = false)
    {
        if (points.Length <= 0)
            return null;

        Line2D[] lines = circle ? new Line2D[points.Length] : new Line2D[points.Length - 1];

        for (int i = 0; i < points.Length - 1; i++)
            lines[i] = new Line2D(points[i], points[i + 1], colors[i]);

        for (int i = 1; i < points.Length - 1; i++)
            lines[i].Previous = lines[i - 1];

        if (circle)
        {
            lines[lines.Length - 1] = new Line2D(points[points.Length - 1], points[0], colors[colors.Length - 1]);
            lines[lines.Length - 1].Previous = lines[lines.Length - 2];
            lines[0].Previous = lines[lines.Length - 1];
        }

        return lines;
    }
}
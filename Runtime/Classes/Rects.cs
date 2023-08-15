using System;
using UnityEngine;

public class Rects
{
    public Rect[] RectsArray => this.rects;

    [SerializeField] Rect[] rects = new Rect[0];
}

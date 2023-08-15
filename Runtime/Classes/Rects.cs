using System;
using UnityEngine;

[System.Serializable]
public class Rects
{
    public Rect[] RectsArray => this.rects;

    [SerializeField] Rect[] rects = new Rect[0];
}

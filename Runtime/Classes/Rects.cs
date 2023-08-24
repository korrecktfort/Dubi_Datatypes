using UnityEngine;

public class Rects : ScriptableObject, IJSON
{
    public Rect[] RectsArray => this.rects;
    public TextAsset TextAsset { get => textAsset; set => textAsset = value; }

    [SerializeField] TextAsset textAsset = null;
    [SerializeField] Rect[] rects = null;
}
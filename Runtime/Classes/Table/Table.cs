using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public abstract class Table<T> : ScriptableObject, IJSON
{
    public TextAsset TextAsset { get => this.textAsset; set => this.textAsset = value; }

    public string[] Titles { get => this.titles; set => this.titles = value; }

    public MultiDimensional<T> Data { get => this.data; set => this.data = value; }


    [SerializeField] TextAsset textAsset = null;
    [SerializeField] string[] titles = new string[0];
    [SerializeField] MultiDimensional<T> data = new MultiDimensional<T>(new T[0][]);

    public Table(params string[] titles)
    {
        this.titles = titles;
        this.data = new MultiDimensional<T>(new T[0][]);
    }

    public T[] Get(int x)
    {
        return this.data.Get(x);
    }

    public T Get(int x, int y)
    {
        return this.data.Get(x, y);
    }

    public Vector2 GetCoordinate(T item)
    {
        return this.data.GetCoordinate(item);
    }

    public bool Contains(T item)
    {
        return this.data.Contains(item);
    }
}

using UnityEngine;
/// <summary>
/// Row of a table.
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class SingleDimensional<T>
{
    [SerializeField] T[] array = new T[0];
    public T[] Array => this.array;

    public SingleDimensional(T[] array)
    {
        this.array = array;
    }
}
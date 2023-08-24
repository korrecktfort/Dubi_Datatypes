using UnityEngine;
using System.Linq;

/// <summary>
/// Table of data.
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class MultiDimensional<T>
{
    public SingleDimensional<T>[] Rows => this.rows;

    [SerializeField] SingleDimensional<T>[] rows = new SingleDimensional<T>[0];

    public MultiDimensional(T[][] table)
    {
        this.rows = new SingleDimensional<T>[table.Length];

        for (int y = 0; y < table.Length; y++)
        {
            this.rows[y] = new SingleDimensional<T>(table[y]);
        }
    }

    public Vector2 GetCoordinate(T item)
    {
        for (int y = 0; y < this.rows.Length; y++)
        {
            if (this.rows[y].Array.ToList().Contains(item))
            {
                return new Vector2(this.rows[y].Array.ToList().IndexOf(item), y);
            }
        }

        return -Vector2.one;
    }

    public bool Contains(T item)
    {
        for (int y = 0; y < this.rows.Length; y++)
        {
            if (this.rows[y].Array.ToList().Contains(item))
            {
                return true;
            }
        }

        return false;
    }

    public T Get(int x, int y)
    {
        return this.rows[y].Array[x];
    }

    public T[] Get(int y)
    {
        return this.rows[y].Array;
    }
}

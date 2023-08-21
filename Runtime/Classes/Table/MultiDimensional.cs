using UnityEngine;
/// <summary>
/// Table of data.
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class MultiDimensional<T>
{
    [SerializeField] SingleDimensional<T>[] rows = new SingleDimensional<T>[0];

    public MultiDimensional(T[][] table)
    {
        this.rows = new SingleDimensional<T>[table.Length];

        for (int y = 0; y < table.Length; y++)
        {
            this.rows[y] = new SingleDimensional<T>(table[y]);
        }
    }

    public int GetRow(T value, int atEntry)
    {
        for (int i = 0; i < this.rows.Length; i++)
        {
            if (this.rows[i].Array.Length <= atEntry)
            {
                return -1;
            }

            if (this.rows[i].Array[atEntry].Equals(value))
            {
                return i;
            }
        }

        return -1;
    }
}

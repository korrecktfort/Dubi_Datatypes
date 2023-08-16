using UnityEngine;

[System.Serializable]
public class Table<T>
{
    #region Subclasses
    [System.Serializable]
    public class MultiDimensional<T>
    {
        public SingleDimensional<T>[] rows = new SingleDimensional<T>[0];

        public MultiDimensional(T[][] table)
        {
            this.rows = new SingleDimensional<T>[table.Length];

            for (int y = 0; y < table.Length; y++)
            {
                this.rows[y] = new SingleDimensional<T>(table[y]);
            }
        }

        public void SetRowEntriesLength(int entries)
        {
            foreach (SingleDimensional<T> row in rows)
                row.SetRowEntriesLength(entries);
        }

        public int GetRow(T value, int atEntry)
        {
            for (int i = 0; i < this.rows.Length; i++)
            {
                if (this.rows[i].array.Length <= atEntry)
                {
                    return -1;
                }

                if (this.rows[i].array[atEntry].Equals(value))
                {
                    return i;
                }
            }

            return -1;
        }
    }

    [System.Serializable]
    public class SingleDimensional<T>
    {
        public T[] array = new T[0];

        public SingleDimensional(T[] array)
        {
            this.array = array;
        }

        public void SetRowEntriesLength(int entries)
        {
            T[] newArray = new T[entries];
            for (int i = 0; i < Mathf.Min(this.array.Length, entries); i++)
            {
                newArray[i] = this.array[i];
            }

            this.array = newArray;
        }
    }
    #endregion

    #region Properties
    public string[] Titles
    {
        get => this.titles;

        set
        {
            this.titles = value;
            this.data.SetRowEntriesLength(titles.Length);
        }
    }

    public SingleDimensional<T>[] Rows
    {
        get => this.data.rows;
        set => this.data.rows = value;
    }
     
    public MultiDimensional<T> Data
    {
        get => this.data;
        set => this.data = value;
    }
    #endregion

    [SerializeField] string[] titles = new string[0];
    [SerializeField] MultiDimensional<T> data = new MultiDimensional<T>(new T[0][]);
}
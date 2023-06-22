namespace Dubi.TableExtension
{
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
}
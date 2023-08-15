using UnityEngine;

  
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

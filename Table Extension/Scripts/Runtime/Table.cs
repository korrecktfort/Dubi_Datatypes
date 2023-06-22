using UnityEngine;

namespace Dubi.TableExtension
{
    [System.Serializable]
    public class Table<T>
    {
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

        [SerializeField] string[] titles = new string[0];
        [SerializeField] MultiDimensional<T> data = new MultiDimensional<T>(new T[0][]); 
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public abstract class Table<T> : ScriptableObject
{
    [SerializeField] TextAsset textAsset = null;
    [SerializeField] string[] titles = new string[0];
    [SerializeField] MultiDimensional<T> data = new MultiDimensional<T>(new T[0][]);

    public Table(params string[] titles)
    {
        this.titles = titles;
        this.data = new MultiDimensional<T>(new T[0][]);
    }

    public void ToJSON()
    {
#if UNITY_EDITOR
        File.WriteAllText(AssetDatabase.GetAssetPath(this.textAsset), JsonUtility.ToJson(this));
#endif
    }

    public void FromJSON()
    {
        JsonUtility.FromJsonOverwrite(this.textAsset.text, this);
    }
}

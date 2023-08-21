using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IJSON
{   
    public TextAsset TextAsset { get; set; }

    public void ToJSON()
    {
#if UNITY_EDITOR
        if (!HasJSONTable())
            return;

        File.WriteAllText(AssetDatabase.GetAssetPath(this.TextAsset), JsonUtility.ToJson(this, true));
#endif
    }

    public void FromJSON()
    {
        if (!HasJSONTable())
            return;

        JsonUtility.FromJsonOverwrite(this.TextAsset.text, this);
    }

    public bool HasJSONTable()
    {
        if (this.TextAsset == null)
            return false;

#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this.TextAsset);

        if (!path.Contains(".json") && !path.Contains(".txt"))
            return false;
#endif

        return true;
    }

    public void OpenJSONFile()
    {
        

#if UNITY_EDITOR
        AssetDatabase.OpenAsset(this.TextAsset);
#endif
    }
}
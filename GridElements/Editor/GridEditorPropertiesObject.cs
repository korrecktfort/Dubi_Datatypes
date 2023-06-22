using Dubi.GridElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dubi/Grid Elements/Persistent Grid Editor Properties")]
public class GridEditorPropertiesObject : ScriptableObject
{
    [System.Serializable]
    public class LocalProperties : GridEditorProperty
    {    
        protected override GridStart GridStartOption => GridStart.Center;

        protected override bool DraggableGrid => true;

        protected override bool SnapToGrid => false;
    }

    [SerializeField] LocalProperties gridProperties = new LocalProperties();
}

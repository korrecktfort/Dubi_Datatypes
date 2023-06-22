using UnityEngine;

namespace Dubi.GridElements
{
    [System.Serializable]
    public abstract class GridEditorProperty : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        protected abstract GridStart GridStartOption { get; }
        protected abstract bool DraggableGrid { get; }
        protected abstract bool SnapToGrid { get; }

        public enum GridStart
        {
            Center,
            TopLeft, 
            TopRight, 
            BottomLeft,
            BottomRight,
        }

        [SerializeField] GridStart gridStart = GridStart.TopLeft;
        [SerializeField] Vector2 gridOffset = Vector2.zero;
        [SerializeField] bool draggableGrid = true;        
#pragma warning disable
        [SerializeField] float zoom = 25.0f;
#pragma warning enable
        [SerializeField] Vector2 elementSize = Vector2.one * 300.0f;
        [SerializeField] bool snapToGrid = true;
        
#endif

        public virtual void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            this.draggableGrid = this.DraggableGrid;
            this.gridStart = this.GridStartOption;
            this.snapToGrid = this.SnapToGrid;
#endif
        }

        public virtual void OnAfterDeserialize()
        {
#if UNITY_EDITOR

#endif
        }        
    }
}
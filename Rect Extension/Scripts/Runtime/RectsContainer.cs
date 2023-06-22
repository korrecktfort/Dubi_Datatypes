using UnityEngine;

namespace Dubi.GridElements
{
    [System.Serializable]
    public class RectsContainer : GridEditorProperty
    {
#if UNITY_EDITOR
        protected override GridStart GridStartOption => GridStart.TopLeft;
        protected override bool DraggableGrid => true;       
        protected override bool SnapToGrid => true;
#endif

        public Rect[] Rects { get => this.rects; }

        [SerializeField] protected Rect[] rects = new Rect[0];
    }
}

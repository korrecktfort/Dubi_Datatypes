using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dubi.Tools.Lines
{
    public class EditorPoints : MonoBehaviour
    {

        [SerializeField] public Vector3[] points = new Vector3[0];
        [SerializeField] public Quaternion[] rotations = new Quaternion[0];
        [SerializeField] public Vector3[] scalings = new Vector3[0];
        [SerializeField] public Lines lines = null;
        [SerializeField] public Curve curve = null;

        public enum Type
        {
            Points = 1,
            Lines = 2,
            Curves = 3,
        }

        [SerializeField] public Type type = Type.Points;
        [SerializeField, HideInInspector] public bool looped = false;
        [SerializeField, HideInInspector] public bool linked = false;

        public System.Action OnReset, OnLineDataRecalculated;

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (Selection.activeObject == this.gameObject)
            {
                return;
            }

            //GizmoDrawer.DrawPoints(this.transform, this.points, this.rotations);
            //GizmoDrawer.DrawLines(GetLines()?.lineArray);
        }
#endif
        public void RecalculateLines()
        {
            switch (this.type)
            {
                case Type.Lines:
                    this.lines = new Lines(this.points, this.looped);
                    break;

                case Type.Curves:
                    this.curve = new Curve(this.points, this.looped);
                    break;
            }

            if (this.OnLineDataRecalculated != null)
            {
                this.OnLineDataRecalculated();
            }
        }

        public Lines GetLines()
        {
            switch (this.type)
            {
                case Type.Lines:
                    return this.lines;

                case Type.Curves:
                    return this.curve.lines;
            }

            return null;
        }

        private void Reset()
        {
            if (this.OnReset != null)
            {
                this.OnReset();
            }
        }
    }
}
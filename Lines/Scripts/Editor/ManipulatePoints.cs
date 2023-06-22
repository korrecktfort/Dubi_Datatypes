using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Dubi.Tools.Lines
{
	public class ManipulatePoints : Editor
	{
		private enum CustomToolType
		{
			None = -1,
			Move = 1,
			Rotate = 2,
			Scale= 3,
		}
		private CustomToolType currentCustomTool = CustomToolType.None;
	
		private EditorPoints editorPoints = null;
		public Vector3[] points = null;
		public Quaternion[] rotations = null;
		public Vector3[] scalings = null;

		private Color handlesColor = Color.cyan;
		private Color selectedHandlesColor = Color.magenta;
		private Color linesColor = Color.cyan;

		private int selectedIndex = -1;
		private float handleSize = 0.125f;
		private Tool cachedTool = Tool.None;

		private Offset offset = null;
		
		private bool linked = false;	
		
		private bool pointsTypeChanged = false;

		private int lastPointsType = 0;
		private bool lastLooped = false;

		public void SetPointsData(EditorPoints editorPoints)
		{
			this.editorPoints = editorPoints;
			this.points = editorPoints.points;
			this.rotations = editorPoints.rotations;
			this.scalings = editorPoints.scalings;

			this.offset = new Offset(editorPoints.transform, editorPoints.points, editorPoints.rotations, this.editorPoints.linked);
			this.editorPoints.OnReset += this.Reset;
		}

		// Update current cached points if Unity Inspector GUI value changes
		public void CheckForPointsDataChange()
		{
			this.points = this.editorPoints.points;		
			this.rotations = this.editorPoints.rotations;
			this.scalings = this.editorPoints.scalings;
		}

		public override void OnInspectorGUI()
		{	
			InputCheck();		

			if (this.editorPoints != null)
			{
				// Inspector GUI
				EditorGUI.BeginDisabledGroup(this.editorPoints.type == EditorPoints.Type.Points);
					this.editorPoints.looped = GUILayout.Toggle(this.editorPoints.looped, this.editorPoints.looped ? "Looped" : "Dead Ends" ,"Button");
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.BeginHorizontal();
				this.editorPoints.linked = GUILayout.Toggle(this.editorPoints.linked, this.editorPoints.linked ? "Linked" : "Unlinked", "Button", GUILayout.MaxWidth(80), GUILayout.Height(38.0f));
				EditorGUILayout.HelpBox("Link Points To GameObject", MessageType.Info);
				EditorGUILayout.EndHorizontal();

				// Inspector GUI Changed
				if (this.editorPoints.linked != this.linked)
				{
					this.offset.Link(this.editorPoints.linked);
					this.linked = this.editorPoints.linked;
				}

				if (this.lastLooped != this.editorPoints.looped)
				{
					this.lastLooped = this.editorPoints.looped;
					CalculateLines();			
					SceneView.RepaintAll();
				}

				if (this.lastPointsType != (int)this.editorPoints.type)
				{
					this.lastPointsType = (int)this.editorPoints.type;
					this.pointsTypeChanged = true;
				
				}
			}
		}

		public void InputCheck()
		{
			if (this.selectedIndex > -1)
			{
				Event e = Event.current;
				UnityEditor.Tools.current = Tool.None;
				if (e.isKey && e.type == EventType.KeyDown)
				{
					if (e.keyCode == KeyCode.W)
					{
						this.currentCustomTool = CustomToolType.Move;
						this.cachedTool = Tool.Move;
					}

					if (e.keyCode == KeyCode.E)
					{
						this.currentCustomTool = CustomToolType.Rotate;
						this.cachedTool = Tool.Rotate;
					}

					if (e.keyCode == KeyCode.R)
					{
						this.currentCustomTool = CustomToolType.Scale;
						this.cachedTool = Tool.Scale;
					}

					if (e.keyCode == KeyCode.Escape)
					{
						this.selectedIndex = -1;
						UnityEditor.Tools.current = cachedTool;
						this.currentCustomTool = (CustomToolType)UnityEditor.Tools.current;
					}

					if (e.keyCode == KeyCode.Delete)
					{
						Vector3 p = this.points[selectedIndex];
						RemovePoint(p);
						e.Use();
					}

					if (e.keyCode == KeyCode.F)
					{
						SceneView.lastActiveSceneView.Frame(new Bounds(this.points[selectedIndex], Vector3.one * this.handleSize * 2.0f), false);
						e.Use();
					}
				}			
			}
		}

		public void UpdateSceneGUI()
		{
			// Change Linked
			bool transformed = OnTransformChanged();				

			// Manipulate Points
			bool manipulated = DrawAndManipulate();

			if (manipulated)
			{
				// Update offset data
				this.offset.UpdateData(this.points, this.rotations);
			}
				
			if (transformed || manipulated || this.pointsTypeChanged)
			{
				// Draw Lines or Curves
				CalculateLines();
				this.pointsTypeChanged = false;
			}

			if (this.editorPoints.type != EditorPoints.Type.Points)
			{
				DrawLines();
			}
		}

		void CalculateLines()
		{
			switch (this.editorPoints.type)
			{
				case EditorPoints.Type.Lines:
					this.editorPoints.lines = new Lines(this.points, this.editorPoints.looped);
					this.editorPoints.curve = null;
					break;

				case EditorPoints.Type.Curves:
					this.editorPoints.lines = null;
					this.editorPoints.curve = new Curve(this.points, this.editorPoints.looped);			
					break;
			}

			if (this.editorPoints.OnLineDataRecalculated != null)
			{
				this.editorPoints.OnLineDataRecalculated();
			}
		}

		public void DrawLines()
		{
			UnityEngine.Rendering.CompareFunction cacheCompare = Handles.zTest;
			Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

			Color cacheColor = this.handlesColor;
			Handles.color = this.linesColor;

			Line[] arrayToDraw = null;

			switch (this.editorPoints.type)
			{
				case EditorPoints.Type.Lines:
					arrayToDraw = this.editorPoints.lines.lineArray;
					break;

				case EditorPoints.Type.Curves:
					arrayToDraw = this.editorPoints.curve.lines.lineArray;
					break;
			}

			if (arrayToDraw != null)
			{
				foreach (Line l in arrayToDraw)
				{
					if (l != null)
					{
						Handles.DrawLine(l.start, l.end);
					}
				}
			}

			Handles.zTest = cacheCompare;
			Handles.color = cacheColor;
		}

		public Vector3[] GetWorldPositions()
		{		
			Transform t = this.editorPoints.transform;
			int length = this.points.Length;
			Vector3[] array = new Vector3[length];
		
			for (int i = 0; i < length; i++)
			{
				array[i] = t.TransformPoint(this.points[i]);
			}
		
			return array;		
		}

		public Quaternion[] GetWorldRotations()
		{
			Transform t = this.editorPoints.transform;
			int length = this.rotations.Length;
			Quaternion[] array = new Quaternion[length];

			for (int i = 0; i < length; i++)
			{
				array[i] = t.rotation * this.rotations[i];
			}

			return array;
		}

		private bool OnTransformChanged()
		{
			if (this.editorPoints != null && this.offset != null && this.editorPoints.linked)
			{
				Transform t = this.editorPoints.transform;
				if (t.hasChanged)
				{
					this.offset.ApplyOffset();
					this.editorPoints.points = this.points = this.offset.positions;
					this.editorPoints.rotations = this.rotations = this.offset.rotations;
					Undo.RecordObject(t, "Changed Transform");
					EditorUtility.SetDirty(t.gameObject);

					t.hasChanged = false;
					return true;
				}
			}

			return false;
		}

		private bool DrawAndManipulate()
		{
			bool valuesChanged = false;

			if (this.points != null)
			{		
				UnityEngine.Rendering.CompareFunction cacheCompare = Handles.zTest;
				Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

				Color cacheColor = this.handlesColor;

				for (int i = 0; i < this.points.Length; ++i)
				{
					if (this.selectedIndex == i)
					{
						cacheColor = Handles.color = this.selectedHandlesColor;
					}
					else
					{
						cacheColor = Handles.color = this.handlesColor;
					}

					Vector3 p = this.points[i];
					Quaternion q = this.rotations[i];

					if (UnityEditor.Tools.pivotRotation == PivotRotation.Local)
					{
						q = this.rotations[i];
					}

					if (Handles.Button(p, q, this.handleSize, this.handleSize, Handles.CubeHandleCap))
					{
						this.selectedIndex = i;

						if (this.selectedIndex == -1)
						{
							this.cachedTool = UnityEditor.Tools.current;
						}
						else
						{
							if (UnityEditor.Tools.current != Tool.Move || UnityEditor.Tools.current != Tool.Rotate || UnityEditor.Tools.current != Tool.Scale)
							{
								UnityEditor.Tools.current = Tool.Move;
							}
						}

						this.currentCustomTool = (CustomToolType)(int)UnityEditor.Tools.current;
					}
				}

				if (this.selectedIndex > -1)
				{					
					Vector3 p = this.points[this.selectedIndex];				
					Quaternion q = this.rotations[this.selectedIndex];

					if (UnityEditor.Tools.pivotRotation == PivotRotation.Global)
					{
						q = Quaternion.identity;
					}

					// Disable z-Test for manipulating gizmos
					Handles.zTest = UnityEngine.Rendering.CompareFunction.Disabled;				
					EditorGUI.BeginChangeCheck();

					if (this.currentCustomTool == CustomToolType.Move)
					{
						p = Handles.DoPositionHandle(p, q);
						if (EditorGUI.EndChangeCheck())
						{
							this.points[this.selectedIndex] = p;
							this.editorPoints.points = this.points;
							Undo.RecordObject(this.editorPoints.gameObject, "Manipulated Transforms");
							EditorUtility.SetDirty(this.editorPoints.gameObject);
							valuesChanged = true;
						}
					}

					if (this.currentCustomTool == CustomToolType.Rotate)
					{
						q = Handles.DoRotationHandle(q, p);
						if (EditorGUI.EndChangeCheck())
						{
							this.rotations[this.selectedIndex] = q;
							this.editorPoints.rotations = this.rotations;
							Undo.RecordObject(this.editorPoints.gameObject, "Manipulated Transforms");
							EditorUtility.SetDirty(this.editorPoints.gameObject);
							valuesChanged = true;
						}
					}

					if (this.currentCustomTool == CustomToolType.Scale)
					{
						Vector3 s = this.scalings[this.selectedIndex];

						s = Handles.DoScaleHandle(s, p, q, 1.0f * HandleUtility.GetHandleSize(p));
						if (EditorGUI.EndChangeCheck())
						{
							this.scalings[this.selectedIndex] = s;
							this.editorPoints.scalings = this.scalings;
							Undo.RecordObject(this.editorPoints.gameObject, "Manipulated Transforms");
							EditorUtility.SetDirty(this.editorPoints.gameObject);
							valuesChanged = true;
						}
					}						
				}

				Handles.zTest = cacheCompare;
				Handles.color = cacheColor;
			}

			return valuesChanged;
		}
	
		public void AddPoint(Vector3 p)
		{		
			List<Vector3> points = this.points.ToList();
			List<Quaternion> rotations = this.rotations.ToList();
			List<Vector3> scalings = this.scalings.ToList();

			points.Add(p);
			rotations.Add(Quaternion.identity);
			scalings.Add(Vector3.one);

			this.editorPoints.points = this.points = points.ToArray();
			this.editorPoints.rotations = this.rotations = rotations.ToArray();
			this.editorPoints.scalings = this.scalings = scalings.ToArray();

			this.offset.UpdateData(this.editorPoints.points, this.editorPoints.rotations);
			CalculateLines();		

			Undo.RecordObject(this.editorPoints.gameObject, "Added Point");
			EditorUtility.SetDirty(this.editorPoints.gameObject);
		}

		public void RemovePoint(Vector3 p)
		{
			List<Vector3> points = this.points.ToList();
			List<Quaternion> rotations = this.rotations.ToList();
			List<Vector3> scalings = this.scalings.ToList();

			if (points.Contains(p))
			{
				int index = points.IndexOf(p);
				points.RemoveAt(index);
				rotations.RemoveAt(index);
				scalings.RemoveAt(index);
			}

			--this.selectedIndex;
			if (this.selectedIndex == -1)
			{
				this.selectedIndex = this.points.Length - 1;
			}

			if (this.points.Length == 0)
			{
				this.points = null;
				this.rotations = null;
				this.scalings = null;
			}

			this.editorPoints.points = this.points = points.ToArray();
			this.editorPoints.rotations = this.rotations = rotations.ToArray();
			this.editorPoints.scalings = this.scalings = scalings.ToArray();

			CalculateLines();
			this.offset.UpdateData(this.editorPoints.points, this.editorPoints.rotations);

			--this.selectedIndex;
			if (this.selectedIndex == -1)
			{
				this.selectedIndex = this.points.Length -1;
			}

			if (this.points.Length == 0)
			{
				this.points = null;
			}

			Undo.RecordObject(this.editorPoints.gameObject, "Removed Point");
			EditorUtility.SetDirty(this.editorPoints.gameObject);
		}
	
		private void OnEnable()
		{		
		}

		private void OnDisable()
		{		
			this.selectedIndex = -1;
			UnityEditor.Tools.current = this.cachedTool;

			if (this.editorPoints != null)
			{
				this.editorPoints.OnReset -= this.Reset;
			}
		}

		public void Reset()
		{
			this.points = null;
			this.rotations = null;
			this.scalings = null;
		}
	}
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dubi.Tools.Lines
{
	public class RaycastHitPoint : Editor
	{
		public delegate void PosOnLeftClick(Vector3 pos);
		public PosOnLeftClick GetRayHitPoint;

		float handleScale = 0.125f;
		Color handlesColor = Color.yellow;
		bool pressed = false;

		Vector3 lastRayHitPos;
		bool rayhit = false;

		public void InputCheck()
		{
			Event e = Event.current;
			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.H && !this.pressed)
			{
				this.pressed = true;			
			}

			if (e.type == EventType.KeyUp && e.keyCode == KeyCode.H && this.pressed)
			{
				this.pressed = false;			
			}            

			if (this.pressed)
			{         
				Vector2 mPos = e.mousePosition;
				Ray ray = HandleUtility.GUIPointToWorldRay(mPos);

				if (Physics.Raycast(ray, out RaycastHit hit, 100.0f, -1))
				{
					this.lastRayHitPos = hit.point;
					this.rayhit = true;

					if (e.type == EventType.MouseUp && e.button == 0 && this.GetRayHitPoint != null)
					{
						this.GetRayHitPoint(this.lastRayHitPos);
						e.Use();
					}				
				}
				else
				{
					this.rayhit = false;
				}					
			}
		}

		public void UpdateSceneGUI()
		{
			if (this.pressed && this.rayhit)
			{			
				int controlID = GUIUtility.GetControlID(FocusType.Passive);

				UnityEngine.Rendering.CompareFunction cacheCompare = Handles.zTest;
				Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
				Color cacheColor = Handles.color;
				Handles.color = this.handlesColor;

				Handles.SphereHandleCap(-1, this.lastRayHitPos, Quaternion.identity, HandleUtility.GetHandleSize(this.lastRayHitPos) * this.handleScale, EventType.Repaint);

				Handles.zTest = cacheCompare;
				Handles.color = cacheColor;
			
				HandleUtility.AddDefaultControl(controlID);
				SceneView.RepaintAll();
			}
		}
	}
}
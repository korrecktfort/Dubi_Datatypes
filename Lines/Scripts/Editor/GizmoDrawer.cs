using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Dubi.Tools.Lines
{
	public static class GizmoDrawer
	{
		public static void DrawLines(params Line[] lines)
		{
			Color cacheColor = Gizmos.color;
			Gizmos.color = Color.cyan;

			if (lines != null)
			{
				foreach (Line l in lines)
				{
					if (l != null)
					{
						Gizmos.DrawLine(l.start, l.end);
					}
				}
			}

			Gizmos.color = cacheColor;
		}

		public static void DrawLines(Line[] lines, Vector3 offset)
		{
			Color cacheColor = Gizmos.color;
			Gizmos.color = Color.cyan;

			if (lines != null)
			{
				foreach (Line l in lines)
				{
					if (l != null)
					{
						Vector3 p0 = l.start + l.right * offset.x + l.up * offset.y + l.forward * offset.z;
						Vector3 p1 = l.end + l.right * offset.x + l.up * offset.y + l.forward * offset.z;
						Gizmos.DrawLine(p0, p1);
					}
				}
			}

			Gizmos.color = cacheColor;
		}

		public static void Draw(this Line l, Color c, float duration)
		{
			Debug.DrawLine(l.start, l.end, c, duration);
		}

		public static void Draw(this Line l)
		{
			Debug.DrawLine(l.start, l.end, Color.yellow);
		}

		public static void DrawPoints(Transform t, Vector3[] points, Quaternion[] rotations = null)
		{
			Color cacheColor = Gizmos.color;
			Gizmos.color = Color.cyan;

			if (points != null)
			{
				for (int i = 0; i < points.Length; i++)
				{
					Vector3 p = points[i];
					Quaternion q = rotations != null ? rotations[i] : Quaternion.identity;

					Matrix4x4 cachedMatrix = Gizmos.matrix;
					Gizmos.matrix = Matrix4x4.TRS(p, q, Vector3.one);

					Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.125f);

					Gizmos.matrix = cachedMatrix;
				}
			}

			Gizmos.color = cacheColor;
		}
	}
}

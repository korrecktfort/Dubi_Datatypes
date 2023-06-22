using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dubi.Tools.Lines
{


	[System.Serializable]
	public class Lines
	{
		Vector3[] points = null;
		public Line[] lineArray = null;
		public float distance = 0.0f;

		public Lines(Vector3[] points, bool looped = false)
		{
			this.points = points;
			this.lineArray = CreateLines(points, looped);
			this.distance = this.lineArray[this.lineArray.Length - 1].endDistance;
		}
		private Line[] CreateLines(Vector3[] points, bool looped = false)
		{
			int length = points.Length;
			List<Line> linesList = new List<Line>();

			Vector3 lastPos = points[0];
			float d = 0.0f;

			for (int i = 1; i < length; i++)
			{
				Vector3 newPos = points[i];
				Line l = new Line(lastPos, newPos);
				l.startDistance = d;
				d += l.distance;
				l.endDistance = d;

				l.startIndex = i - 1;
				l.endIndex = i;

				linesList.Add(l);
				lastPos = newPos;
			}

			if (looped)
			{
				Line l = new Line(lastPos, points[0]);
				l.startDistance = d;
				d += l.distance;
				l.endDistance = d;

				l.startIndex = points.Length - 1;
				l.endIndex = 0;

				linesList.Add(l);
			}

			return linesList.ToArray();
		}

		public Line GetLine(float distance)
		{
			foreach (Line l in this.lineArray)
			{
				if (l.startDistance <= distance && distance < l.endDistance)
				{
					return l;
				}
			}

			return null;
		}

		public Line GetNextLine(Line current)
		{
			return this.lineArray[current.endIndex];
		}

		public Line GetPreviousLine(Line current)
		{
			int index = current.startIndex - 1 < 0 ? this.lineArray.Length - 1 : current.startIndex - 1;
			return this.lineArray[index];
		}
	}
}
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Dubi.Functions;

namespace Dubi.Tools.Lines
{
    [System.Serializable]
	public class Curve
	{
		int iterations = 16;
		public Lines lines = null;
		public CurveSegment[] curveSegments = null;
		public bool looped = false;

		public Curve(Vector3[] points, bool looped = false)
		{
			this.looped = looped;
			this.lines = CreateLines(points, looped);
			this.curveSegments = CreateSegments(points.Length, this.lines, looped);
		}

		public CurveSegment[] CreateCurve(Vector3[] points, bool looped = false)
		{
			this.lines = CreateLines(points, looped);
			this.curveSegments = CreateSegments(points.Length, this.lines, looped);
			return this.curveSegments;
		}

		private Lines CreateLines(Vector3[] points, bool looped = false)
		{
			int length = points.Length;

			if (!looped && length > 1)
			{
				// Add hidden mirrored endpoints on both ends for curves till the visible endpoints

				// Resize points array and add hidden points
				Vector3[] array = new Vector3[length + 2];

				array[0] = points[0] - (points[1] - points[0]);
				array[length + 1] = points[length - 1] + (points[length - 1] - points[length - 2]);

				for (int pIndex = 1; pIndex <= length; pIndex++)
				{
					array[pIndex] = points[pIndex - 1];
				}

				points = array;
				length = points.Length;
			}

			//List<Line> lines = new List<Line>();

			//float d = 0.0f;
			//int index = 0;
			List<Vector3> positions = new List<Vector3>();

			for (int i = 0; i < length; i++)
			{
				if ((i == 0 || i == length - 2 || i == length - 1) && !looped)
				{
					continue;
				}

				Vector3 p0 = points[ClampIndex(i - 1, length)];
				Vector3 p1 = points[ClampIndex(i, length)];
				Vector3 p2 = points[ClampIndex(i + 1, length)];
				Vector3 p3 = points[ClampIndex(i + 2, length)];

				//Vector3 lastPos = p1;
				positions.Add(p1);

				float resolution = 1.0f / (float)this.iterations;

				for (int j = 1; j < this.iterations; j++)
				{
					float t = j * resolution;
					positions.Add(Math.GetCatmullRomPosition(t, p0, p1, p2, p3));									
				}

				positions.Add(p2);				
			}

			Lines lines = new Lines(positions.ToArray(), looped);
			return lines;
		}

		int ClampIndex(int i, int length)
		{
			if (i < 0)
			{
				return length - 1;
			}

			if (i > length)
			{
				return 1;
			}

			else if (i > length - 1)
			{
				return 0;
			}

			return i;
		}

		public CurveSegment[] CreateSegments(int length, Lines lines, bool looped)
		{
			List<CurveSegment> segmentsList = new List<CurveSegment>();


			float d = 0.0f;

			for (int i = 1; i < length; i++)
			{
				int start = (i - 1) * iterations;

				CurveSegment segment = GetFromRange(start, iterations, lines.lineArray.ToList());
				segment.SetCollectionData(i - 1, i, d);
				d += segment.distance;
				segmentsList.Add(segment);
			}

			if (looped)
			{
				int start = (length - 1) * iterations;
				CurveSegment segment = GetFromRange(start, iterations, lines.lineArray.ToList());
				segment.SetCollectionData(length - 1, 0, d);
				segmentsList.Add(segment);
			}

			return segmentsList.ToArray();
		}		

		public CurveSegment GetFromRange(int start, int range, List<Line> lines)
		{
			Line[] linesPartArray = lines.GetRange(start, range).ToArray();
			return new CurveSegment(linesPartArray);
		}

		public CurveSegment GetSegment(float distance)
		{
			float d = 0.0f;

			foreach (CurveSegment s in this.curveSegments)
			{
				if (distance > d && distance < d + s.distance)
				{
					return s;
				}

				d += s.distance;
			}

			return null;
		}

		public CurveSegment GetNextSegment(CurveSegment segment)
		{
			return this.curveSegments[segment.endIndex];
		}

		public CurveSegment GetPreviousSegment(CurveSegment segment)
		{
			int index = ClampIndex(segment.startIndex - 1, this.curveSegments.Length);
			return this.curveSegments[index];
		}
	}
}
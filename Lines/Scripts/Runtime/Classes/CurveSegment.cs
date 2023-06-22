namespace Dubi.Tools.Lines
{
    [System.Serializable]
	public class CurveSegment
	{
		public Line[] lines = null;
		public float distance = 0.0f;

		// if part of curve collection
		public int startIndex = 0;
		public int endIndex = 0;

		public float startDistance = 0.0f;
		public float endDistance = 0.0f;

		public CurveSegment(Line[] lines)
		{
			this.lines = lines;
			this.distance = GetDistance();
		}

		public void SetCollectionData(int startIndex, int endIndex, float startDistance)
		{
			this.startIndex = startIndex;
			this.endIndex = endIndex;
			this.startDistance = startDistance;
			this.endDistance = startDistance + this.distance;
		}

		public float GetDistance()
		{
			float d = 0.0f;

			foreach (Line l in this.lines)
			{
				d += l.distance;
			}

			return d;
		}

		public Line GetLine(float distance)
		{
			float d = this.startDistance;

			foreach (Line l in this.lines)
			{
				if (distance > d && distance < d + l.distance)
				{
					return l;
				}

				d += l.distance;
			}

			return null;
		}
	}
}
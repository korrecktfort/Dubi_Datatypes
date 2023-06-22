using UnityEngine;

namespace Dubi.Tools.Lines
{
	[System.Serializable]
	public class Line
	{
		/// <summary>
		/// Class Line
		/// </summary>		
		public Vector3 start;
		public Vector3 end;
		public float lineRotation = 0.0f;
		public Vector3 right, up, forward, center;
		public Quaternion lookRotation;
		public float distance;
		public Vector3 horizontalDirection;
		public float horizontalDistance;

		public float nDistanceFactor = 0.0f;

		// If part of a collection
		public int startIndex;
		public int endIndex;
		public float startDistance;
		public float endDistance;

		public Line(Vector3 start, Vector3 end)
		{
			this.start = start;
			this.end = end;
			ApplyData();
		}

		public Line(Vector3 dir)
		{
			this.start = Vector3.zero;
			this.end = dir;
			ApplyData();
		}

		public Line(Line line)
		{
			this.start = line.start;
			this.end = line.end;
			this.lineRotation = line.lineRotation;
			this.forward = line.forward;
			this.right = line.right;
			this.up = line.up;
			this.center = line.center;
			this.distance = line.distance;
			this.lookRotation = line.lookRotation;
			this.nDistanceFactor = line.nDistanceFactor;
			ApplyData();
		}

		public Line(Line line, float lineRotation)
		{
			this.start = line.start;
			this.end = line.end;
			this.lineRotation = lineRotation;
			ApplyData();
		}

		public Line(Vector3 start, Vector3 end, float lineRotation)
		{
			this.start = start;
			this.end = end;
			this.lineRotation = lineRotation;
			ApplyData();
		}

		public void ApplyData()
		{
			this.forward = Forward();
			this.right = Right();
			this.up = Up();
			this.distance = Distance();
			this.center = Center();
			this.lookRotation = LookRotation();
			this.horizontalDirection = HorizontalDirection();
			this.horizontalDistance = HorizontalDistance();
			this.nDistanceFactor = 1.0f / this.distance;
		}

		public void SwitchDirection()
		{
			Vector3 start = this.end;
			Vector3 end = this.start;
			this.end = start;
			this.start = end;
			ApplyData();
		}

		public Vector3 Forward()
		{
			return (this.end - this.start).normalized;
		}

		public void Forward(Vector3 newForward)
		{
			this.end = this.start + newForward * this.distance;
			ApplyData();
		}

		public Vector3 Right()
		{

			float _dot = Vector3.Dot(Vector3.up, this.forward);
			Vector3 dir = Vector3.zero;

			if (Mathf.Abs(_dot) > 0.99999f)
			{
				dir = Vector3.Cross(Vector3.forward, this.forward).normalized;
				dir = Quaternion.AngleAxis(this.lineRotation, this.forward) * dir;
				return dir;
			}

			dir = Vector3.Cross(Vector3.up, this.forward).normalized;
			dir = Quaternion.AngleAxis(this.lineRotation, this.forward) * dir;
			return dir;
		}

		public static Vector3 Right(Vector3 forward)
		{
			float dot = Vector3.Dot(Vector3.up, forward);

			if (Mathf.Abs(dot) > 0.99999f)
			{
				return Vector3.Cross(Vector3.forward, forward).normalized;
			}

			return Vector3.Cross(Vector3.up, forward).normalized;
		}

		public static Vector3 Up(Vector3 forward, Vector3 right)
		{
			return Vector3.Cross(forward, right).normalized;
		}

		public Vector3 Up()
		{
			return Vector3.Cross(this.forward, this.right).normalized;
		}

		public float Distance()
		{
			return (this.end - this.start).magnitude;
		}

		public void Distance(float newDistance)
		{
			this.end = this.start + this.forward * newDistance;
			ApplyData();
		}

		public Vector3 Center()
		{
			Vector3 v;
			Vector3 s = this.start;
			Vector3 f = this.forward;
			float d = this.distance * 0.5f;

			v.x = s.x + f.x * d;
			v.y = s.y + f.y * d;
			v.z = s.z + f.z * d;

			return v;
		}

		public Quaternion LookRotation()
		{
			// Prevent "Look Rotation is Zero" Messages and Behaviour
			if (this.forward != Vector3.zero && this.up != Vector3.zero)
			{
				return Quaternion.LookRotation(this.forward, this.up);
			}

			return Quaternion.identity;
		}

		public void LookAt(Transform transform)
		{
			this.end = transform.position;
			ApplyData();

		}

		public void LineRotation(float newRotation)
		{
			this.lineRotation = newRotation;
			ApplyData();
		}

		public Vector3 HorizontalDirection()
		{
			Vector3 horDir = this.forward;
			horDir.y = 0;
			horDir.Normalize();
			return horDir;
		}

		public float HorizontalDistance()
		{
			Vector3 s = this.start;
			Vector3 e = this.end;

			s.y = 0;
			e.y = 0;

			return Vector3.Distance(s, e);
		}

		public void RepeatAlongLine(System.Action<Vector3> action, float distanceBetweenReps, bool alwaysIncludeEnd)
		{
			int repetitions = (int)(this.distance / distanceBetweenReps);
			Vector3 targetPosition;

			for (int index = 0; index < repetitions; ++index)
			{
				targetPosition.x = this.start.x + this.forward.x * index * distanceBetweenReps;
				targetPosition.y = this.start.y * this.forward.y * index * distanceBetweenReps;
				targetPosition.z = this.start.z * this.forward.z * index * distanceBetweenReps;

				action(targetPosition);
			}

			if (alwaysIncludeEnd)
			{
				action(this.end);
			}
		}

		public void RepeatAlongLine(System.Action<Vector3> action, int repetitions)
		{
			Vector3 targetPosition;
			float alpha = 0;
			for (int index = 0; index < repetitions; ++index)
			{
				alpha = (float)index / (repetitions - 1);
				targetPosition.x = this.start.x + this.forward.x * alpha * this.distance;
				targetPosition.y = this.start.y * this.forward.y * alpha * this.distance;
				targetPosition.z = this.start.z * this.forward.z * alpha * this.distance;

				action(targetPosition);
			}
		}

		public bool RaycastAlongLine(LayerMask mask)
		{
			return Physics.Linecast(this.start, this.end, mask);
		}

		public bool RaycastAlongLine(LayerMask mask, out RaycastHit hit)
		{
			return Physics.Linecast(this.start, this.end, out hit, mask);
		}

		// K.R.: Source: https://stackoverflow.com/questions/51905268/how-to-find-closest-point-on-line
		public Vector3 FindNearestPointOnLineFrom(Vector3 point)
		{
			Vector3 lhs = point - this.start;
			float dot = Vector2.Dot(lhs, this.forward);
			dot = Mathf.Clamp(dot, 0f, distance);
			return this.start + this.forward * dot;
		}

		public Vector3 PosAtNDistance(float progress)
		{
			return this.start + this.forward * progress * this.distance;
		}

		public Vector3 PosAtDistance(float distance)
		{
			return this.start + this.forward * Mathf.Clamp(distance, 0.0f, this.distance);
		}
	}
}
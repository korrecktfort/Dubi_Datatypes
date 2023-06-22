using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dubi.Tools.Lines
{
	[RequireComponent(typeof(Accelerator))]
	public class MoveAlongLines : MonoBehaviour
	{
		public EditorPoints editorPoints;
		Lines lines;
		Line currentLine;

		Accelerator acc;
		public Accelerator.AcceleratorValues accValues;

		float currentDistance = 0.0f;
		
		private void Awake()
		{
			this.acc = GetComponent<Accelerator>();		

			this.lines = this.editorPoints?.GetLines();
			this.currentLine = this.lines.GetLine(this.currentDistance);
		}

		private void OnEnable()
		{
			this.editorPoints.OnLineDataRecalculated += UpdateLineData;
		}

		private void OnDisable()
		{
			this.editorPoints.OnLineDataRecalculated -= UpdateLineData;
		}

		private void Start()
		{		
			this.acc.ChangeSpeedTo(this.accValues);
		}

		private void Update()
		{
			Move();
		}

		private void UpdateLineData()
		{
			this.lines = this.editorPoints.GetLines();
			this.currentLine = this.lines.GetLine(this.currentDistance);
		}

		private void Move()
		{
			if (this.currentLine != null)
			{
				this.currentDistance += this.acc.speed * Time.deltaTime;

				if (this.currentDistance > this.lines.distance)
				{
					this.currentDistance -= this.lines.distance;

					// backup plan if the last distance step was multiple times the lines distance
					if (this.currentDistance > this.lines.distance)
					{
						float factor = this.currentDistance / this.lines.distance;
						float flatFactor = Mathf.Floor(factor);
						this.currentDistance -= flatFactor * this.lines.distance;

						this.currentLine = this.lines.GetLine(this.currentDistance);
					}
				}

				if (this.currentDistance < 0.0f)
				{
					this.currentDistance += this.lines.distance;

					// backup plan if the last distance step was multiple times the lines distance
					if (this.currentDistance <= 0.0f)
					{
						float factor = this.currentDistance / this.lines.distance;
						float flatFactor = Mathf.Floor(Mathf.Abs(factor));
						this.currentDistance += flatFactor * this.lines.distance;

						this.currentLine = this.lines.GetLine(this.currentDistance);
					}
				}

				if (this.currentDistance > this.currentLine.endDistance)
				{
					this.currentLine = this.lines.GetNextLine(currentLine);

					// backup plan if last distance step skipped some lines
					if (this.currentDistance > this.currentLine.endDistance)
					{
						this.currentLine = this.lines.GetLine(this.currentDistance);
					}
				}

				if (this.currentDistance < this.currentLine.startDistance)
				{
					this.currentLine = this.lines.GetPreviousLine(currentLine);

					// backup plan if the last distance stepp skipped some lines
					if (this.currentDistance < this.currentLine.startDistance)
					{
						this.currentLine = this.lines.GetLine(this.currentDistance);
					}
				}

				float deltaDistance = this.currentDistance - this.currentLine.startDistance;
				Vector3 newPos = this.currentLine.PosAtDistance(deltaDistance);
				this.transform.position = newPos;
			}
		}
	}
}

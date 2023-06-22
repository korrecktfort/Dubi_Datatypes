using UnityEditor;

namespace Dubi.Tools.Lines
{
	[CustomEditor(typeof(EditorPoints))]
	public class EditorPointsEditor : Editor
	{
		EditorPoints editorPoints;
		ManipulatePoints manipulatePoints;
		RaycastHitPoint raycastHitPoint;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			this.manipulatePoints.OnInspectorGUI();
		}

		private void OnSceneGUI()
		{
			this.raycastHitPoint.InputCheck();
			this.raycastHitPoint.UpdateSceneGUI();

			this.manipulatePoints.InputCheck();
			this.manipulatePoints.UpdateSceneGUI();
		}

		private void OnEnable()
		{
			this.editorPoints = (EditorPoints)target;

			if (this.editorPoints != null)
			{
				this.manipulatePoints = CreateInstance<ManipulatePoints>();
				this.manipulatePoints.SetPointsData(this.editorPoints);

				this.raycastHitPoint = CreateInstance<RaycastHitPoint>();
				this.raycastHitPoint.GetRayHitPoint -= this.manipulatePoints.AddPoint;
				this.raycastHitPoint.GetRayHitPoint += this.manipulatePoints.AddPoint;


			}
		}

		private void OnDisable()
		{
			this.editorPoints = null;
		}


	}
}
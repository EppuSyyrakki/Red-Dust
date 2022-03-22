using UnityEngine;

namespace RedDust.Control.Input
{
	public struct CamInput
	{
		public Vector2 movement;
		public float rotation;
		public float zoom;

		/// <summary>
		/// Lerps the values towards values in the parameter struct.
		/// </summary>
		public void LerpTo(CamInput target)
		{
			movement.x = Mathf.Lerp(movement.x, target.movement.x, Time.deltaTime * Values.Camera.AccelMove);
			movement.y = Mathf.Lerp(movement.y, target.movement.y, Time.deltaTime * Values.Camera.AccelMove);
			rotation = Mathf.Lerp(rotation, target.rotation, Time.deltaTime * Values.Camera.AccelRot);
			zoom = Mathf.Lerp(zoom, target.zoom, Time.deltaTime * Values.Camera.AccelZoom);
		}
	}
}
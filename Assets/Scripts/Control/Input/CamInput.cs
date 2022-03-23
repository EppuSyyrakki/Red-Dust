using UnityEngine;

namespace RedDust.Control.Input
{
	public struct CamInput
	{
		public Vector2 pan;
		public float rotation;
		public float zoom;

		/// <summary>
		/// Lerps the values towards values in the parameter struct.
		/// </summary>
		public void LerpTo(CamInput target)
		{
			pan.x = Mathf.Lerp(pan.x, target.pan.x, Time.deltaTime * Values.Camera.AccelMove);
			pan.y = Mathf.Lerp(pan.y, target.pan.y, Time.deltaTime * Values.Camera.AccelMove);
			rotation = Mathf.Lerp(rotation, target.rotation, Time.deltaTime * Values.Camera.AccelRot);
			zoom = Mathf.Lerp(zoom, target.zoom, Time.deltaTime * Values.Camera.AccelZoom);
		}
	}
}
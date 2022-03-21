using System;
using UnityEngine;

namespace Utils
{
	/// <summary>
	/// Raycasts into given direction with the given range. Triggers an event that returns a normal from
	/// the first collider hit in given layers.
	/// </summary>
	[DefaultExecutionOrder(-10000)]
	public class RaycastForNormal : MonoBehaviour
	{
		[SerializeField]
		private Vector3 offset = Vector3.zero;

		[SerializeField]
		private Vector3 direction = Vector3.down;

		[SerializeField]
		private float range = 1f;

		[SerializeField]
		private LayerMask layerMask;

		[SerializeField]
		private bool debugRaycast;

		private Timer _timer = null;

		public event Action<Vector3> CastPerformed;

		private void Awake()
		{
			_timer = new Timer(1f);
		}

		private void OnEnable()
		{
			_timer.Alarm += RaycastGroundNormal;
		}

		private void OnDisable()
		{
			_timer.Alarm -= RaycastGroundNormal;
		}

		private void Update()
		{
			_timer.Tick();
		}

		private void RaycastGroundNormal()
		{
			Vector3 position = transform.position + offset;

			if (debugRaycast) 
			{ 
				Debug.DrawLine(position, position + direction.normalized * range, Color.red, _timer.AlarmTime); 
			}
			
			if (!Physics.Raycast(position, direction.normalized, out RaycastHit hit, range, layerMask)) { return; }

			CastPerformed?.Invoke(hit.normal);
		}

		public void SetTimer(float time, bool reset = true)
		{
			_timer.SetAlarm(time);

			if (reset) { _timer.Reset(); }
		}

		public void SetDirection(Vector3 direction)
		{
			this.direction = direction;
		}
	}
}
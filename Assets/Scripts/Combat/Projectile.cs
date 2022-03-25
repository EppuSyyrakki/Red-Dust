using System;
using UnityEngine;

namespace RedDust.Combat
{
	public class Projectile : MonoBehaviour
	{
		[SerializeField]
		private float velocity;

		[SerializeField]
		private int penetration;

		[SerializeField]
		private float damage;

		private float maxTravel = Values.Combat.MaxProjectileTravel;
		private readonly int maxRaycastHits = Values.Combat.MaxRaycastHits;
		private readonly int projectilesLayer = Values.Layer.Projectiles;

		public event Action<Projectile> Finished;

		public int Penetration { get; private set; }
		public float Travelled { get; private set; }

		/// <summary>
		/// Holds direction, distance (delta * velocity), origin, layerMask and maxHits (damage left).
		/// </summary>
		public RaycastCommand Info { get; private set; }
		public int SenderId { get; private set; }

		public void Init(Vector3 origin, Vector3 direction, int senderId)
		{
			transform.forward = direction;
			var vel = velocity * Time.deltaTime;
			Info = new RaycastCommand(origin, direction, vel, projectilesLayer, maxRaycastHits);
			SenderId = senderId;
			Penetration = penetration;
		}

		/// <summary>
		/// Call after the job in AttackScheduler is complete.
		/// </summary>
		/// <param name="penetration"></param>
		public void Set(RaycastCommand result, int penetration, float travelled)
		{
			Info = new RaycastCommand(result.from, result.direction, velocity * Time.deltaTime,
				result.layerMask, result.maxHits);
			Penetration = penetration;
			Travelled = travelled;
			transform.position = Info.from;

			if (Travelled > maxTravel)
			{
				Finished?.Invoke(this);
			}
		}
	}
}
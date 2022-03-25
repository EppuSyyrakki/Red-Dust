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

		private float maxTravel;
		private readonly int maxRaycastHits = Values.Combat.MaxRaycastHits;
		private readonly int projectilesLayer = Values.Layer.Projectiles;

		public event Action<Projectile> ProjectileFinished;

		public int Penetration { get; private set; }
		public float Travelled { get; private set; }

		/// <summary>
		/// Holds direction, distance (delta * velocity), origin, layerMask and maxHits (damage left).
		/// </summary>
		public RaycastCommand Info { get; private set; }
		public int SenderId { get; private set; }

		public void Init(Vector3 origin, Vector3 direction, int senderId)
		{
			var vel = velocity * Time.deltaTime;
			Info = new RaycastCommand(origin, direction, vel, projectilesLayer, maxRaycastHits);
			SenderId = senderId;
			Penetration = penetration;
			maxTravel = Values.Combat.MaxProjectileTravel;
		}

		/// <summary>
		/// Call after the job in AttackScheduler is complete.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="penetration"></param>
		public void Set(RaycastCommand info, int penetration, float travelled)
		{
			Info = new RaycastCommand(info.from, info.direction, velocity * Time.deltaTime,
				info.layerMask, info.maxHits);
			transform.position = Info.from + Info.direction;
			Penetration = penetration;
			Travelled = travelled;

			if (Travelled > maxTravel)
			{
				ProjectileFinished?.Invoke(this);
			}
		}
	}
}
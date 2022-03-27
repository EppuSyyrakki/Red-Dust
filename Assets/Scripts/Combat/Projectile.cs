using System;
using UnityEngine;

namespace RedDust.Combat
{
	public class Projectile : MonoBehaviour
	{
		private const int maxRaycastHits = Values.Combat.MaxRaycastHits;
		private const int projectilesLayer = Values.Layer.ProjectileHits;
		private const float maxTravel = Values.Combat.MaxProjectileTravel;

		[SerializeField]
		[Tooltip("Speed of the projectile in unity units per second.")]
		private float velocity;

		[SerializeField]
		[Tooltip("The amount of armor penetration. Reduced each time it goes through something.")]
		private int penetration;

		[SerializeField]
		[Tooltip("Amount of health lost on hit. Is reduced on hit by the amount of penetration lost.")]
		private int damage;

		[SerializeField]
		[Tooltip("Effect components that will be added to what was hit.")]
		private Effect[] effects;
		
		/// <summary>
		/// Event that is fired when the projectile can be queued for removal in the scheduler.
		/// </summary>
		public event Action<Projectile> Finished;

		public int Penetration { get; private set; }
		public int Damage { get; private set; }
		public float Travelled { get; private set; }
		public Effect[] Effects { get; private set; }
		public RaycastHit Hit { get; set; }
		public int LayerHitFirst { get; set; }	

		public RaycastCommand Info { get; private set; }
		public int SenderId { get; private set; }

		public void Init(Vector3 origin, Vector3 direction, int senderId, int wpnPenetration, Effect[] effects)
		{
			transform.forward = direction;
			var vel = velocity * Time.deltaTime;
			Info = new RaycastCommand(origin, direction, vel, projectilesLayer, maxRaycastHits);
			SenderId = senderId;		
			Penetration = penetration + wpnPenetration;
			Damage = damage;
			Effects = effects;
		}

		public void Set(RaycastCommand result, int penetration, int damage, float travelled)
		{
			bool hasHit = Hit.normal != Vector3.zero;

			// The result array has already calculated the new position. Velocity must be calculated again.
			// The layer mask parameter tells us what layer were hit, if any.
			Info = new RaycastCommand(result.from, result.direction, velocity * Time.deltaTime,
				projectilesLayer, result.maxHits);
			LayerHitFirst = result.layerMask;
			Travelled = travelled;
			Damage = damage;
			Penetration = penetration;

			if ((hasHit && (Penetration <= 0 || Damage <= 0)) || Travelled > maxTravel)
			{
				// The projectile has hit something and lost all its penetration power or damage,
				// or travelled far enough to be removed from the game.
				Finished?.Invoke(this);
				transform.position = Hit.point;
				return;
			}

			transform.position = Info.from;
		}
	}
}
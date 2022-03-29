using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Combat
{
	[System.Serializable]
	public struct HealthStatus
	{
		public int current;
		public int max;
		public int armor;
		
		public HealthStatus(int current, int max, int armor)
		{
			this.current = current;
			this.max = max;
			this.armor = armor;
		}
	}

	public abstract class Health : MonoBehaviour
	{
		[SerializeField]
		private bool logHealth = false;

		[SerializeField]
		private HealthStatus startingHealth = new HealthStatus(1, 1, 0);

		[SerializeField]
		protected HealthStatus status;

		public Transform TargetingTransform { get; protected set; }
		public HealthStatus Status { get => status; }
		public List<Effect> Effects { get; private set; } = new List<Effect>();
		public bool IsDead => status.current <= 0;

		public virtual void Awake()
		{
			status = startingHealth;
		}

		/// <summary>
		/// Subtracts health by amount. Armor should be calculated elsewhere (in the ProjectileScheduler when
		/// handling hits).
		/// </summary>
		/// <param name="amount">The amount to reduce health</param>
		/// <returns>True if health reduced to 0</returns>
		public bool TakeDamage(int amount)
		{
			status.current = Mathf.Clamp(status.current - amount, 0, status.current);

			if (logHealth)
			{
				Debug.Log($"{gameObject.name} took {amount} damage, {status.current}/{status.max} remaining");
			}

			if (status.current == 0)
			{
				Kill();
				return true;
			}

			return false;
		}

		public void AddEffect(Effect effect)
		{
			Effects.Add(effect);

			if (logHealth)
			{
				Debug.Log($"{gameObject.name} had {effect.name} applied to them.");
			}
		}

		public abstract void Kill();
	}
}
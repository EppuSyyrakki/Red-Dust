using RedDust.Combat;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public class ShootAction : ActionBase
	{
		private Vector3 target;
		private Health targetHealth;
		private float timer;
		private bool facingTarget;

		public ShootAction(Character c, Vector3 target, Health targetHealth = null) 
			: base(c)
		{
			this.target = target;
			this.targetHealth = targetHealth;
		}

		public override ActionState Execute()
		{
			timer += Time.deltaTime;

			if (!Character.Mover.TurnTowards(target) 
				&& timer > Character.Fighter.AttackFrequency)
			{
				Character.Fighter.Shoot(target);
				timer = 0;
			}	

			if (targetHealth != null && targetHealth.IsDead)
			{
				return ActionState.Success;
			}

			return ActionState.Running;
		}
	}
}

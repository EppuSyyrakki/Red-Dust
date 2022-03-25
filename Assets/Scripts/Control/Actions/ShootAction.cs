using RedDust.Combat;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public class ShootAction : ActionBase
	{
		private Vector3 target;
		private Health targetHealth;
		private float attackFreq;
		private float timer;

		public ShootAction(Character c, Vector3 target, float attackFreq, Health targetHealth = null) 
			: base(c)
		{
			this.target = target;
			this.targetHealth = targetHealth;
			this.attackFreq = attackFreq;
		}

		public override ActionState Execute()
		{
			timer += Time.deltaTime;

			if (timer > attackFreq)
			{
				Character.Fighter.Attack(target);
				timer = 0;
			}

			if (targetHealth != null && targetHealth.Current == 0)
			{
				return ActionState.Success;
			}

			return ActionState.Running;
		}
	}
}

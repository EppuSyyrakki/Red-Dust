using RedDust.Combat;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public class ShootAction : ActionBase
	{
		private Vector3 target;
		private Health targetHealth;
		private float timer;

		/// <summary>
		/// Create a shoot action against something that has a health component.
		/// </summary>
		public ShootAction(Character c, Health targetHealth) : base(c)
		{
			this.targetHealth = targetHealth;
			target = this.targetHealth.TargetingTransform.position;
		}

		/// <summary>
		/// Create a shoot action attacking objects without health components.
		/// </summary>
		public ShootAction(Character c, Vector3 target) : base(c)
        {
			this.target = target;
			targetHealth = null;
        }

		public override void OnStart()
		{
			base.OnStart();
			Character.Motion.BlendCombat(true);
		}

		public override ActionState Execute()
		{
			timer += Time.deltaTime;

			if (targetHealth == null)
            {
				AimTurnShoot(target);
			}
			else
            {
				AimTurnShoot(targetHealth.TargetingTransform.position);
            }					

			if (targetHealth != null && targetHealth.IsDead)
			{
				return ActionState.Success;
			}

			return ActionState.Running;
		}

		public override void OnEnd()
		{
			Character.Motion.BlendCombat(false);
		}

		private void AimTurnShoot(Vector3 target)
        {
			Character.Motion.Aim(target);

			if (!Character.Motion.TurnTowards(target)
			&& timer > Character.Fighter.AttackFrequency)
			{
				Character.Fighter.Shoot(target);
				timer = 0;
			}
		}
	}
}

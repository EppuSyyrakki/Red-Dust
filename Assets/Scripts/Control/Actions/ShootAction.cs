using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public class ShootAction : ActionBase
	{
		Vector3 target;
		float startTime;
		float attackSpeed;

		public ShootAction(Character c, Vector3 target, float attackSpeed) : base(c)
		{
			this.target = target;
			this.attackSpeed = attackSpeed;
		}

		public override void OnStart()
		{
			base.OnStart();
			startTime = Time.time;
		}

		public override ActionState Execute()
		{
			if (Time.time > startTime + attackSpeed)
			{
				Character.Fighter.Attack(target);
				return ActionState.Success;
			}

			return ActionState.Running;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public class IdleAction : ActionBase
	{
		private float waitTime;
		private float timer = 0;

		public IdleAction(Character c, float waitTime = 0) : base(c)
		{
			this.waitTime = waitTime;
		}

		public override ActionState Execute()
		{
			if (waitTime > 0) 
			{
				timer += Time.deltaTime;

				if (timer > waitTime) { return ActionState.Success; }
			}

			return ActionState.Idle;
		}
	}
}

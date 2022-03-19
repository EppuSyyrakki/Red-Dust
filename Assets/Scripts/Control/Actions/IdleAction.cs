using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public class IdleAction : ActionBase
	{
		public IdleAction(Character c) : base(c)
		{
		}

		public override ActionState Execute()
		{
			return ActionState.Idle;
		}
	}
}

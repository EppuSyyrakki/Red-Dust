using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public class IdleAction : ActionBase
	{
		public IdleAction(CharacterControl c) : base(c)
		{

		}

		public override ActionState Execute()
		{
			return ActionState.Idle;
		}

		public override void OnFailure()
		{
			
		}

		public override void OnSuccess()
		{
			
		}
	}
}
